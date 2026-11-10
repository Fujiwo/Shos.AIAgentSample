// 新: このステップで追加したファイル (AI エージェント本体)
// このファイルで使う NuGet パッケージを追加するコマンド
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

namespace FCAIChat.AIAgents;

// AI エージェントの基底クラス (チャットクライアントの生成・MCP サーバーへの接続・会話のセッションの管理を行う)
// エージェント名・指示・使うモデル (チャットクライアント)・MCP のツールは、派生クラスで決める
public abstract class ChatAgent : IAsyncDisposable
{
    // 接続中の MCP クライアント (破棄するときに使う) と、エージェントを開始済みかどうか
    IEnumerable<McpClient> mcpClients           = [];
    bool                   isStarted            = false;
    // 同じエージェントへの同時呼び出し (同じユーザーの複数タブなど) を直列化する
    readonly SemaphoreSlim gate                 = new(1, 1);

    AIAgent?               agent                = null;
    // 複数ターンに対応するために AgentSession (会話の状態・履歴などを管理) を作成
    AgentSession?          session              = null;

    // エージェント名と指示
    public abstract string    Name         { get; }
    protected abstract string Instructions { get; }
    // エージェントのシステムロールに与える文脈的な指示
    protected abstract string SystemPrompt { get; }

    // エージェントを破棄する (MCP クライアントを破棄して、MCP サーバーのプロセスを終了する)
    public async ValueTask DisposeAsync()
    {
        // 実行中の呼び出しが終わるのを待ってから、MCP クライアントを破棄
        await gate.WaitAsync();
        try {
            await DisposeMcpClientsAsync(mcpClients);
            mcpClients = [];
            agent      = null;
            session    = null;
            isStarted  = false;
        } finally {
            gate.Release();
        }
        GC.SuppressFinalize(this);
    }

    // ユーザーのプロンプトに対するエージェントの応答を取得する (最初の呼び出し時にエージェントを開始する)
    public async Task<string> GetResponseAsync(string userPrompt)
    {
        await gate.WaitAsync();
        try {
            try {
                await StartAsync();
            } catch (Exception ex) {
                // MCP サーバーの起動失敗や環境変数の未設定など
                Debug.WriteLine($"Failed to start agent: {ex}");
                return $"(エージェントを開始できませんでした: {ex.Message})";
            }
            // ユーザープロンプトをエージェントのセッションに送信し、応答を取得
            return await RunAgentAsync(new ChatMessage(ChatRole.User, userPrompt));
        } finally {
            gate.Release();
        }
    }

    // ファクトリ関数: エージェントが使うチャットクライアントを生成する (派生クラスで実装する)
    protected abstract IChatClient GetChatClient();

    // エージェントが使う MCP のツールを取得する (既定ではツールなし。派生クラスでオーバーライドする)
    protected virtual Task<(IEnumerable<McpClient>, IEnumerable<McpClientTool>)> GetMcpToolsAsync()
        => Task.FromResult<(IEnumerable<McpClient>, IEnumerable<McpClientTool>)>(([], []));

    // MCP クライアントを破棄する (破棄に失敗しても、残りの MCP クライアントの破棄を続ける)
    protected static async Task DisposeMcpClientsAsync(IEnumerable<McpClient> mcpClients)
    {
        foreach (var mcpClient in mcpClients) {
            try {
                await mcpClient.DisposeAsync();
            } catch (Exception ex) {
                Debug.WriteLine($"Failed to dispose MCP client: {ex.Message}");
            }
        }
    }

    // エージェントを開始する (MCP のツールを取得し、エージェントとセッションを作成する)。開始済みなら何もしない
    async Task StartAsync()
    {
        if (isStarted)
            return;

        var (mcpClients, tools) = await GetMcpToolsAsync();
        try {
            agent = CreateAgent(tools);

            session = await agent.CreateSessionAsync();

            // システムメッセージを最初に送信
            await RunAgentAsync(new ChatMessage(ChatRole.System, SystemPrompt));
        } catch {
            // 開始に失敗したら MCP クライアントを破棄し、次回の呼び出しで再試行する
            await DisposeMcpClientsAsync(mcpClients);
            agent   = null;
            session = null;
            throw;
        }

        this.mcpClients = mcpClients;
        isStarted       = true;

        // チャットクライアントと MCP のツールを使う ChatClientAgent を作成
        AIAgent CreateAgent(IEnumerable<McpClientTool> tools)
            => new ChatClientAgent(
                chatClient  : GetChatClient(),
                instructions: Instructions,
                name        : Name,
                description : null,
                tools       : tools.Any() ? tools.Cast<AITool>().ToList() : null
            );
    }

    // エージェントに ChatMessage を投げて応答を取得
    async Task<string> RunAgentAsync(ChatMessage chatMessage)
    {
        if (agent is null)
            return string.Empty;

        try {
            var response = await agent.RunAsync(chatMessage, session);
            return response?.Text ?? string.Empty;
        } catch (Exception ex) {
            Debug.WriteLine($"Error running agent: {ex}");
            return $"(エラー: {ex.Message})";
        }
    }
}

// このアプリで使う AI エージェント (Azure OpenAI のモデルと、MCP サーバーのツールを使う)
public class MyChatAgent : ChatAgent
{
    // エージェント名と指示
    public const string       AgentName    = "Agent";
    public override string    Name         => AgentName;
    protected override string Instructions => "あなたはAIエージェントです";
    // エージェントのシステムロールに与える文脈的な指示
    protected override string SystemPrompt => "あなたはAIエージェントです";

    // Azure OpenAI のチャットクライアントを生成する
    protected override IChatClient GetChatClient()
    {
        // 使用するモデルを指定
        const string deploymentName = "MAI-Thinking-1";
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

        // Azure OpenAI のエンドポイントを環境変数から取得
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

        // Azure OpenAI の APIキーを環境変数から取得
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

    // MCP サーバーのツールを取得
    // - McpClient に接続し、ツール一覧を取得して返す
    // - 戻り値は (IEnumerable<McpClient>, IEnumerable<McpClientTool>) で、終了時にすべての McpClient について DisposeAsync() を呼ぶ必要がある
    protected override async Task<(IEnumerable<McpClient>, IEnumerable<McpClientTool>)> GetMcpToolsAsync()
    {
        McpClient client = await McpClient.CreateAsync(GetPlaywrightToolClientTransport());
        try {
            IList<McpClientTool> tools = await client.ListToolsAsync();
            foreach (var tool in tools)
                Debug.WriteLine($"{tool.Name} ({tool.Description})");
            return ([client], tools);
        } catch {
            // ツール一覧の取得に失敗したら、起動した MCP サーバーを破棄する
            await client.DisposeAsync();
            throw;
        }

        // Playwright ツールを使うためのクライアント生成
        static IClientTransport GetPlaywrightToolClientTransport()
            => new StdioClientTransport(new() {
                Name      = "playwright-mcp",
                Command   = "npx"           ,
                Arguments = ["@playwright/mcp@latest"]
            });
    }
}
