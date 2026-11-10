/*
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
dotnet add package ModelContextProtocol --prerelease
dotnet add package OllamaSharp
 */

#define AZURE_OPENAI
#if AZURE_OPENAI
// Azure OpenAI のクライアント用
using Azure;
using Azure.AI.OpenAI;
#else // AZURE_OPENAI
// Ollama 用
using OllamaSharp;
#endif // AZURE_OPENAI

// Microsoft Agent Framework 用
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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
    // 複数ターンに対応するために AgentSession (会話の状態・履歴などを管理) を作成
    public AgentSession?      Session      { get; set; }
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

        static ChatMessage ToUserMessage(string userPrompt) => new(ChatRole.User, userPrompt);
    }

    // ファクトリ関数
    protected abstract IChatClient GetChatClient();
    
    protected virtual Task<(IEnumerable<McpClient>, IEnumerable<AITool>)> GetToolsAsync()
        => Task.FromResult<(IEnumerable<McpClient>, IEnumerable<AITool>)>(([], []));

    async Task Start()
    {
        if (!isFirst)
            return;

        var (mcpClients, tools) = await GetToolsAsync();
        try {
            Agent   = CreateAgent(tools);
            Session = await Agent.CreateSessionAsync();

            // システムメッセージを最初に送信
            await SendSystemMessageAsync();
        } catch {
            // 開始に失敗したら MCP クライアントを破棄し、次回の呼び出しで再試行する
            foreach (var mcpClient in mcpClients)
                await mcpClient.DisposeAsync();
            Agent   = null;
            Session = null;
            throw;
        }

        this.mcpClients = mcpClients;
        isFirst         = false;

        AIAgent CreateAgent(IEnumerable<AITool> tools)
                => new ChatClientAgent(
                    chatClient  : GetChatClient(),
                    instructions: Instructions,
                    name        : Name,
                    description : null,
                    tools       : tools.Any() ? tools.ToList() : null
                );

        // システムメッセージを作成して送信
        async Task SendSystemMessageAsync() => await RunAgentAsync(new(ChatRole.System, SystemPrompt));
    }

    // エージェントに ChatMessage を投げて応答を取得
    async Task<string> RunAgentAsync(ChatMessage chatMessage)
    {
        if (Agent is null)
            return string.Empty;

        try {
            var response = await Agent.RunAsync(chatMessage, Session);
            return response?.Text ?? string.Empty;
        } catch (Exception ex) {
            Debug.WriteLine($"Error running agent: {ex.Message}");
            return "……";
        }
    }
}

public class MyChatAgent : ChatAgent
{
    // エージェント名と指示
    public override string    Name         => "CADオペレーター兼イラストレーター";   
    protected override string Instructions => "CADオペレーター兼イラストレーターとして、CADを用いて様々な製図やイラストを描いてください。";
    // エージェントのシステムロールに与える文脈的な指示
    protected override string SystemPrompt => @$"
あなたは一流のCADオペレーター兼イラストレーターです。CADを用いて様々な製図やイラストを描くことを得意としています。
目的: 指示に従って美しく読みやすく分かりやすい図面や絵を描いてください。

- 特に指示がなければ、幅{Toolbox.GetPaperSize().Width}・高さ{Toolbox.GetPaperSize().Height}の作図領域を基準にします
- 特に指示がなければ、描かれたものは消さないでください (クリアしないでください)
- 特に指示がなければ、多めに描いてください
- 常にプロフェッショナルな品質を追求してください

作業手順:
1. 要望を読み取り、必要なら不足情報(寸法、方位、視点など)を適宜補ってください。
2. 図面全体の構成を短くプランニングし、主要要素と使用するツール(API)を考えてください。
3. 描画時は座標(左上を原点、右方向がX+、下方向がY+)や単位を意識してください。
4. 線は `DrawLine`、矩形は `DrawRectangle`、角丸矩形は `DrawRoundedRectangle`、円は `DrawCircle`、楕円は `DrawEllipse`、折れ線や多角形は `DrawPolylineOrPolygon`、滑らかな曲線は `DrawCurve`、自由曲線や複雑な輪郭は `DrawFreeFormCurve` を用いて描き、必要に応じて `ClearAll` でリセットします。
   各ツールは描画結果を返します。`Error` で始まる結果が返った場合は、内容を確認して指定を直してください。
5. 色は、有効な色 ({GetAvailableColorNameList()}) から指定してください。
6. 色・線種・重ね順を適切に選んでください。
7. 線幅は3から30の範囲で指定し、図面の目的に応じて適切な太さを選んでください。
8. 必要に応じて `GetPaperSize` で用紙サイズを確認し、適切なスケールで描画してください。
9. ユーザーから指示があるまでは何も描画しないでください。
10. ユーザーから指示があった場合は、直ちに描画してください。
";
    static string GetAvailableColorNameList()
    {
        var availableColorNames = Toolbox.GetAvailableColors();
        return string.Join(", ", availableColorNames);
    }

#if AZURE_OPENAI
    //Azure OpenAI を使う場合のクライアント生成
    protected override IChatClient GetChatClient()
    {
        // 使用するモデルを指定
        const string deploymentName = "MAI-Thinking-1";
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

            // 上記のように、セキュリティ上 Azure OpenAI のエンドポイントは環境変数から取得するのが望ましい。ハードコードする場合は、上記の代わりに以下のように書く
            // 例: https://your-resource-name.openai.azure.com/
            //return @"[Azure OpenAI のエンドポイント]";
        }

        static string GetKey()
        {
            const string AzureOpenAIApiKeyEnvironmentVariable = "AZURE_OPENAI_API_KEY";
            var openAIApiKey = Environment.GetEnvironmentVariable(AzureOpenAIApiKeyEnvironmentVariable);
            if (string.IsNullOrEmpty(openAIApiKey))
                throw new InvalidOperationException($"Please set the {AzureOpenAIApiKeyEnvironmentVariable} environment variable.");
            return openAIApiKey;

            // 上記のように、セキュリティ上 Azure OpenAI の APIキーは環境変数から取得するのが望ましい。ハードコードする場合は、上記の代わりに以下のように書く
            // 例: 1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef
            //return @"[Azure OpenAI の APIキー]";
        }
    }
#else // AZURE_OPENAI
    // Ollama を使う場合のクライアント生成(ローカルの Ollama サーバーに接続)
    protected override IChatClient GetChatClient()
    {
        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);
        // 使用するモデルを指定
        // クラウドベースのモデルを使用(実行速度の向上のため)
        // ローカル LLM を使用する場合は "gemma3:latest" などに変更してください
        ollama.SelectedModel = "minimax-m3:cloud";

        // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
        IChatClient chatClient = ollama;
        chatClient = chatClient.AsBuilder()
                               .UseFunctionInvocation() // ツール呼び出しを使う
                               .Build();
        return chatClient;
    }
#endif // AZURE_OPENAI

    // CAD 操作用のツールと、MCP サーバーのツールを取得
    // - Brave Search の MCP サーバーは任意 (接続できない場合は CAD ツールだけで動作する)
    // - 戻り値は (IEnumerable<McpClient>, IEnumerable<AITool>) で、終了時にすべての McpClient について DisposeAsync() を呼ぶ必要がある
    protected override async Task<(IEnumerable<McpClient>, IEnumerable<AITool>)> GetToolsAsync()
    {
        McpClient? client = null;
        try {
            client = await McpClient.CreateAsync(GetBraveSearchToolClientTransport());
            IList<McpClientTool> mcpTools = await client.ListToolsAsync();
            foreach (var tool in mcpTools)
                Debug.WriteLine($"{tool.Name} ({tool.Description})");

            return ([client], [.. GetCadTools(), .. mcpTools.Cast<AITool>()]);
        } catch (Exception ex) {
            // BRAVE_API_KEY が未設定、npx が見つからない、などの場合
            Debug.WriteLine($"Brave Search MCP server is not available: {ex.Message}");
            if (client is not null)
                await client.DisposeAsync();
            return ([], GetCadTools());
        }

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

                // 上記のように、セキュリティ上 Brave の APIキーは環境変数から取得するのが望ましい。ハードコードする場合は、上記の代わりに以下のように書く
                //return @"[Brave の APIキー]";
            }
        }

        // CAD 操作用のツール群を取得
        static IEnumerable<AITool> GetCadTools()
            => [AIFunctionFactory.Create(Toolbox.GetPaperSize         ),
                AIFunctionFactory.Create(Toolbox.GetAvailableColors   ),
                AIFunctionFactory.Create(Toolbox.ClearAll             ),
                AIFunctionFactory.Create(Toolbox.DrawLine             ),
                AIFunctionFactory.Create(Toolbox.DrawRoundedRectangle ),
                AIFunctionFactory.Create(Toolbox.DrawRectangle        ),
                AIFunctionFactory.Create(Toolbox.DrawCircle           ),
                AIFunctionFactory.Create(Toolbox.DrawEllipse          ),
                AIFunctionFactory.Create(Toolbox.DrawPolylineOrPolygon),
                AIFunctionFactory.Create(Toolbox.DrawCurve            ),
                AIFunctionFactory.Create(Toolbox.DrawFreeFormCurve    )];
    }
}
