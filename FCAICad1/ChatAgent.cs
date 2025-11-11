/*
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
dotnet add package ModelContextProtocol --prerelease
dotnet add package OllamaSharp
 */

// Azure OpenAI のクライアント用
using Azure;
using Azure.AI.OpenAI;
// Microsoft Agent Framework 用
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
// Ollama 用
using OllamaSharp;
// MCP クライアントとツールを利用するための名前空間
using ModelContextProtocol.Client;
// Debug.WriteLine を使うための名前空間
using System.Diagnostics;

namespace FCAICad;

public abstract class ChatAgent : IAsyncDisposable
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

    public async ValueTask DisposeAsync()
    {
        // 終了処理 MCP クライアントを破棄
        foreach (var mcpClient in mcpClients)
            await mcpClient.DisposeAsync();
    }

    public async Task<string> GetResponseAsync(string userPrompt)
    {
        await Start();
        // ユーザープロンプトをエージェントスレッドに送信し、応答を取得
        return await RunAgentAsync(ToUserMessage(userPrompt)) ?? string.Empty;

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

        // システムメッセージを作成して送信
        async Task SendSystemMessageAsync() => await RunAgentAsync(new (ChatRole.System, SystemPrompt));
    }

    // エージェントに ChatMessage を投げて応答を取得
    async Task<string> RunAgentAsync(ChatMessage chatMessage)
    {
        if (Agent is null)
            return string.Empty;

        try {
            var response = await Agent.RunAsync(chatMessage, Thread);
            return response?.Text ?? string.Empty;
        } catch (Exception ex) {
            Debug.WriteLine($"Error running agent: {ex.Message}");
            return string.Empty;
        }
    }
}

public class MyChatAgent : ChatAgent
{
    // エージェント名と指示
    public override string    Name         => "CADオペレーター";   
    protected override string Instructions => "CADオペレーターとして、CADを用いた製図を行ってください。";
    // エージェントのシステムロールに与える文脈的な指示
    protected override string SystemPrompt => @$"
あなたは一流のCADオペレーター兼イラストレーターです。CADを用いて様々な製図やイラストを描くことを得意としています。
目的: 指示に従って美しく読みやすく分かりやすい図面や絵を描いてください。
特に指示がなければ、幅{Program.GetPaperSize().Width}・高さ{Program.GetPaperSize().Height}の作図領域を基準にします。

作業手順:
1. 要望を読み取り、必要なら不足情報(寸法、方位、視点など)を適宜補ってください。
2. 図面全体の構成を短くプランニングし、主要要素と使用するツール(API)を考えてください。
3. 描画時は座標(左上を原点、右方向がX+、下方向がY+)や単位を意識してください。
4. 線は `DrawLine`、円は `DrawCircle`、楕円は `DrawEllipse`、自由曲線や複雑な輪郭は `DrawFreeLine` を用いて描き、必要に応じて `ClearAll` でリセットします。
5. 色・線種・重ね順を適切に選び、視認性向上のためにコントラストのある色を選択してください。
6. 必要に応じて `GetPaperSize` で用紙サイズを確認し、適切なスケールで描画してください。
7. 指示を待たずに、直ちに描画してください。

常にプロフェッショナルな品質を追求してください。
";

    //// Ollama を使う場合のクライアント生成(ローカルの Ollama サーバーに接続)
    //protected override IChatClient GetChatClient()
    //{
    //    var uri = new Uri("http://localhost:11434");
    //    var ollama = new OllamaApiClient(uri);
    //    // 使用するモデルを指定
    //    // クラウドベースのモデルを使用(実行速度の向上のため)
    //    // ローカル LLM を使用する場合は "gemma3:latest" などに変更してください
    //    ollama.SelectedModel = "gpt-oss:20b-cloud";

    //    // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
    //    IChatClient chatClient = ollama;
    //    chatClient = chatClient.AsBuilder()
    //                           .UseFunctionInvocation() // ツール呼び出しを使う
    //                           .Build();
    //    return chatClient;
    //}

    //Azure OpenAI を使う場合のクライアント生成
    protected override IChatClient GetChatClient()
    {
        // 使用するモデルを指定
        const string deploymentName = "gpt-5-mini";
        var azureOpenAIEndPoint = GetEndPoint();
        var openAIApiKey = GetKey();
        var credential = new AzureKeyCredential(openAIApiKey);

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
                        Name                 = "braveSearch",
                        Command              = "npx",
                        Arguments            = ["-y", "@modelcontextprotocol/server-brave-search"],
                        EnvironmentVariables = new Dictionary<string, string?> {
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
            => [AIFunctionFactory.Create(Program.GetPaperSize),
                AIFunctionFactory.Create(Program.ClearAll    ),
                AIFunctionFactory.Create(Program.DrawLine    ),
                AIFunctionFactory.Create(Program.DrawCircle  ),
                AIFunctionFactory.Create(Program.DrawEllipse ),
                AIFunctionFactory.Create(Program.DrawFreeLine)];
    }
}
