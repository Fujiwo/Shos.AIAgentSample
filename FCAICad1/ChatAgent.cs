/*
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
dotnet add package ModelContextProtocol --prerelease
 */

// Microsoft Agent Framework 用
// Azure OpenAI のクライアント用
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
// MCP クライアントとツールを利用するための名前空間
using ModelContextProtocol.Client;
// Debug.WriteLine を使うための名前空間
using System.Diagnostics;

namespace FCAICad;

public abstract class ChatAgent : IDisposable
{
    IEnumerable<McpClient> mcpClients           = [];
    bool                   isFirst              = true;

    // AIAgent を公開してスレッドのシリアライズに使用
    public AIAgent?           Agent        { get; private set; }
    // 複数ターンに対応するために AgentThread (会話の状態・履歴などを管理) を作成
    public AgentThread?       Thread       { get; set; } = null;
    // エージェント名と指示
    public abstract string    Name         { get; }
    protected abstract string Instructions { get; }
    // エージェントのシステムロールに与える文脈的な指示
    protected abstract string SystemPrompt { get; }

    public async void Dispose()
    {
        // 終了処理 MCP クライアントを破棄
        foreach (var mcpClient in mcpClients)
            await mcpClient.DisposeAsync();
    }

    public async Task<string> GetResponseAsync(string userPrompt)
    {
        await Start();

        // ユーザープロンプトをエージェントスレッドに送信し、応答を取得
        var response = Agent is null ? null : await Agent.RunAsync(ToUserMessage(userPrompt), Thread);
        return response?.Text ?? string.Empty;

        static ChatMessage ToUserMessage(string userPrompt) => new ChatMessage(ChatRole.User, userPrompt);
    }

    // ファクトリ関数
    protected abstract IChatClient GetChatClient();
    
    protected virtual async Task<(IEnumerable<McpClient>, IEnumerable<AITool>)> GetToolsAsync()
    {
        await Task.Delay(0);
        return ([], []);
    }

    async Task Start()
    {
        if (!isFirst)
            return;

        var (mcpClients, tools) = await GetToolsAsync();
        this.mcpClients = mcpClients;

        Agent = CreateAgent(tools);
        Thread = Agent.GetNewThread();

        // システムメッセージを最初に送信 (only for new threads)
        await SendSystemMessageAsync();

        isFirst = false;

        AIAgent CreateAgent(IEnumerable<AITool> tools)
        {
            var chatClientAgentOptions = new ChatClientAgentOptions { Name = Name, Instructions = Instructions };
            if (tools.Any())
                // ツールをエージェントに渡す
                chatClientAgentOptions.ChatOptions = new ChatOptions { Tools = tools.ToList() };

            return new ChatClientAgent(GetChatClient(), chatClientAgentOptions);
        }

        async Task SendSystemMessageAsync()
        {
            // システムメッセージを作成して送信
            ChatMessage systemMessage = new(ChatRole.System, SystemPrompt);
            if (Agent is not null)
                await Agent.RunAsync(systemMessage, Thread);
        }
    }
}

public class MyChatAgent : ChatAgent
{
    // エージェント名と指示
    public override string    Name         => "CADオペレーター";   
    protected override string Instructions => "CADオペレーターとして、CADを用いた製図を行ってください。";
    // エージェントのシステムロールに与える文脈的な指示
    protected override string SystemPrompt => "あなたは一流のCADオペレーターです。CADを用いた様々な製図を得意としています。指示に従って図面を描いてください。";

    protected override IChatClient GetChatClient()
    {
        // 使用するモデルを指定
        const string deploymentName = "gpt-5-mini";
        var azureOpenAIEndPoint     = GetEndPoint();
        var openAIApiKey            = GetKey();
        var credential              = new AzureKeyCredential(openAIApiKey);

        var azureOpenAIClient = new AzureOpenAIClient(new Uri(azureOpenAIEndPoint), credential);
        // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
        IChatClient chatClient = azureOpenAIClient.GetChatClient(deploymentName)
                                                  .AsIChatClient()
                                                  .AsBuilder()
                                                  .UseFunctionInvocation() // ツール呼び出しを使う
                                                  .Build();
        return chatClient;

        static string GetEndPoint()
        {
            const string AzureOpenAIEndpointEnvironmentVariable = "AZURE_OPENAI_ENDPOINT";
            var azureOpenAIEndPoint = Environment.GetEnvironmentVariable(AzureOpenAIEndpointEnvironmentVariable);
            if (string.IsNullOrEmpty(azureOpenAIEndPoint))
                throw new InvalidOperationException($"Please set the {AzureOpenAIEndpointEnvironmentVariable} environment variable.");
            return azureOpenAIEndPoint;

            // 上記のように、セキュリティ上 Azure OpenAI のエンドポイントは環境変数から取得するのが望ましいが、ここではハードコードする
            // 例: 1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef
            //return @"[Azure OpenAI のエンドポイント]";
        }

        static string GetKey()
        {
            const string AzureOpenAIApiKeyEnvironmentVariable = "AZURE_OPENAI_API_KEY";
            var openAIApiKey = Environment.GetEnvironmentVariable(AzureOpenAIApiKeyEnvironmentVariable);
            if (string.IsNullOrEmpty(openAIApiKey))
                throw new InvalidOperationException($"Please set the {AzureOpenAIApiKeyEnvironmentVariable} environment variable.");
            return openAIApiKey!;

            // 上記のように、セキュリティ上 Azure OpenAI の APIキーは環境変数から取得するのが望ましいが、ここではハードコードする
            //例: https://your-resource-name.openai.azure.com/
            //return @"[Azure OpenAI の APIキー]";
        }
    }

    // MCP サーバーのツールを取得
    // - McpClient に接続し、ツール一覧を取得して返す
    // - 戻り値は (IEnumerable<McpClient>, IEnumerable<McpClientTool>) で、終了時にすべての McpClient について DisposeAsync() を呼ぶ必要がある
    protected override async Task<(IEnumerable<McpClient>, IEnumerable<AITool>)> GetToolsAsync()
    {
        IClientTransport clientTransport = GetBraveSearchToolClientTransport();
        McpClient client = await McpClient.CreateAsync(clientTransport);
        IList<McpClientTool> mcpTools = await client.ListToolsAsync();
        foreach (var tool in mcpTools)
            Debug.WriteLine($"{tool.Name} ({tool.Description})");

        return ([client], [.. GetCadTools(), ..mcpTools.Cast<AITool>()]);

        // BraveSearch ツールを使うためのクライアント生成
        static IClientTransport GetBraveSearchToolClientTransport()
        {
            return new StdioClientTransport(new() {
                        Name = "braveSearch",
                        Command = "npx",
                        Arguments = ["-y", "@modelcontextprotocol/server-brave-search"],
                        EnvironmentVariables = new Dictionary<string, string> {
                            ["BRAVE_API_KEY"] = GetKey()
                        }
                   });

            static string GetKey()
            {
                const string braveApiKeyEnvironmentVariable = "BRAVE_API_KEY";
                var braveApiKey = Environment.GetEnvironmentVariable(braveApiKeyEnvironmentVariable);
                if (string.IsNullOrEmpty(braveApiKey))
                    throw new InvalidOperationException($"Please set the {braveApiKeyEnvironmentVariable} environment variable.");
                return braveApiKey;

                // 上記のように、セキュリティ上 Brave の APIキーは環境変数から取得するのが望ましいが、ここではハードコードする
                //return @"[Brave の APIキー]";
            }
        }

        // CAD 操作用のツール群を取得
        static IEnumerable<AITool> GetCadTools()
        {
            return new[] {
                AIFunctionFactory.Create(Program.GetPaperSize),
                AIFunctionFactory.Create(Program.DrawLine    )
            };
        }
    }
}
