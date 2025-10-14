using System;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
// Azure OpenAI のクライアントを利用するための名前空間
using Azure;
using Azure.AI.OpenAI;
// MCP クライアントとツールを利用するための名前空間
using ModelContextProtocol.Client;
// Debug.WriteLine を使うための名前空間
using System.Diagnostics;

// MCP サーバー (STDIO) を利用するAI エージェントの実行例
// - 指定されたチャットクライアント（Ollama / Azure OpenAI）を作成
// - ChatClientAgent を使った対話を行う
// - ChatClientAgent には、複数の MCP サーバーのツールを渡して利用
//
// 主な流れ:
// 1) 使用するチャットクライアントを作成
// 2) 複数の MCP サーバー (Time, Weather, FileSystem) へ接続してツールを取得
// 3) 取得したツールを ChatClientAgent に渡してエージェントを作成
// 4) AgentThread を使った複数ターン対話ループを実行
// 5) 終了時に全ての MCP クライアントを破棄
//
// 注意:
// - セキュリティ上、実運用では API キーやエンドポイントは環境変数やシークレットストアから読み込むことが望ましい
// - STDIO トランスポートはローカルプロセス間通信向け

// エージェント名と指示
const string agentName    = "AIエージェント";
const string instructions = "あなたはAIエージェントです";
// エージェントのシステムロールに与える文脈的な指示
const string systemPrompt = "あなたはAIエージェントです";

// 使用するチャットクライアント種別
const ChatClientType chatClientType = ChatClientType.AzureOpenAI;
IChatClient chatClient = GetChatClient(chatClientType);

// MCP サーバー群のツールを取得
// 旧:
//var (mcpClient, tools) = await GetMcpServerTools();
// 新:
var (mcpClients, tools) = await GetAllMcpServerTools();

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = agentName,
        Instructions = instructions,
        // ツールをエージェントに渡す
        ChatOptions = new ChatOptions { Tools = tools.Cast<AITool>().ToList() }
    }
);

// 複数ターンに対応するために AgentThread (会話の状態・履歴などを管理) を作成
AgentThread thread = agent.GetNewThread();

// システムメッセージを作成して最初に送信
ChatMessage systemMessage = new(ChatRole.System, systemPrompt);
await RunAsync(agent, systemMessage, thread);

const string exitPrompt = "exit";
Console.WriteLine($"(Interactive chat started. Type '{exitPrompt}' to quit.)\n");

// 対話ループ: ユーザー入力を受け取り exit で終了
for (; ;) {
    var (isValid, userMessage) = GetUserMessage();
    if (!isValid)
        break;
    await RunAsync(agent, userMessage, thread);
}

// 終了処理 MCP クライアントを破棄
// 旧:
//await mcpClient.DisposeAsync();
// 新: ここから
foreach (var mcpClient in mcpClients)
    await mcpClient.DisposeAsync();
// 新: ここまで

// エージェントに ChatMessage を投げて応答を取得
static async Task RunAsync(AIAgent agent, ChatMessage chatMessage, AgentThread? thread = null)
{
    try {
        var response = await agent.RunAsync(chatMessage, thread);
        Console.WriteLine($"Agent: {response.Text ?? string.Empty}\n");
    } catch (Exception ex) {
        Console.WriteLine($"Error running agent: {ex.Message}");
    }
}

// コンソールからユーザー入力を読み取り ChatMessage を返す
static (bool isValid, ChatMessage userMessage) GetUserMessage()
{
    var (isValid, userPrompt) = GetUserPrompt();
    return (isValid, new(ChatRole.User, userPrompt));

    static (bool isValid, string userPrompt) GetUserPrompt()
    {
        Console.Write("You: ");
        var userPrompt = Console.ReadLine();
        Console.WriteLine();

        return string.IsNullOrWhiteSpace(userPrompt) ||
               string.Equals(userPrompt.Trim(), exitPrompt, StringComparison.OrdinalIgnoreCase)
            ? (isValid: false, userPrompt: string.Empty)
            : (isValid: true, userPrompt: userPrompt!);
    }
}

// Ollama を使う場合のクライアント生成（ローカルの Ollama サーバーに接続）
static IChatClient GetOllamaClient()
{
    var uri    = new Uri("http://localhost:11434");
    var ollama = new OllamaApiClient(uri);
    // 使用するモデルを指定
    ollama.SelectedModel = "gpt-oss:20b-cloud"; // ここでは実行速度の都合でクラウドのものを選択しているが、ローカルLLMの場合は "gemma3:latest" など

    // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
    IChatClient chatClient = ollama;
    chatClient = chatClient.AsBuilder()
                           .UseFunctionInvocation() // ツール呼び出しを使う
                           .Build();
    return chatClient;
}

// Azure OpenAI を使う場合のクライアント生成
static IChatClient GetAzureOpenAIClient()
{
    var azureOpenAIEndPoint     = GetEndPoint();
    var openAIApiKey            = GetKey();
    var credential              = new AzureKeyCredential(openAIApiKey);
    // 使用するモデルを指定
    const string deploymentName = "gpt-5-mini";

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
        // [Azure OpenAI のエンドポイント] の部分は、実際のもので置き換えてください
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
        // [Azure OpenAI の APIキー] の部分は、実際のもので置き換えてください
        //return @"[Azure OpenAI の APIキー]";
    }
}

// ChatClientType に基づいて適切な IChatClient を返すファクトリ関数
static IChatClient GetChatClient(ChatClientType chatClientType)
    => chatClientType switch {
        ChatClientType.Ollama      => GetOllamaClient     (),
        ChatClientType.AzureOpenAI => GetAzureOpenAIClient(),
        _ => throw new NotSupportedException($"Chat client type '{chatClientType}' is not supported.")
    };


// 旧: ここから
//// MCP サーバー (STDIO) のツールを取得
//// - STDIO トランスポート経由で McpClient に接続し、ツール一覧を取得して返す
//// - 戻り値は (McpClient, IEnumerable<McpClientTool>) で、終了時に McpClient.DisposeAsync() を呼ぶ必要がある
//static async Task<(McpClient, IEnumerable<McpClientTool>)> GetMcpServerTools()
//{
//    IClientTransport clientTransport = GetTimeToolClientTransport();
//    McpClient client = await McpClient.CreateAsync(clientTransport);

//    IList<McpClientTool> tools = await client.ListToolsAsync();
//    foreach (var tool in tools)
//        Debug.WriteLine($"{tool.Name} ({tool.Description})");
//    return (client, tools);
//}

//// MCP サーバー (STDIO) を使うためのクライアント生成
//// - STDIO 経由で MCP サーバー（Time ツール）に接続するためのトランスポート
//// - Command/Arguments を適切に設定して、MCP サーバー プロジェクトを起動
//// - 実際のプロジェクトパスは環境に合わせて更新してください
//static IClientTransport GetTimeToolClientTransport()
//    => new StdioClientTransport(new() {
//        Name = "time",
//        Command = "dotnet",
//        // [MCPServer.Con.csprojのフルパス] の部分は、実際のもので置き換えてください
//        //Arguments = ["run", "--project", @"[MCPServer.Con.csprojのフルパス]"]
//        Arguments = ["run", "--project", @"C:\DropBox\Dropbox\Source\GitHub\Repos\2025.10.AIAgentsSeminarSlide\Shos.AIAgentSample\MCPServer.Con\MCPServer.Con.csproj"]
//    });
// 旧: ここまで

// 新: ここから
// 複数の MCP サーバーのツールを取得
// - 複数の McpClient に接続し、ツール一覧を取得して返す
// - 戻り値は (IEnumerable<McpClient>, IEnumerable<McpClientTool>) で、終了時にすべての McpClient について DisposeAsync() を呼ぶ必要がある
static async Task<(IEnumerable<McpClient>, IEnumerable<McpClientTool>)> GetAllMcpServerTools()
{
    var (timeMcpClient      , timeTools      ) = await GetMcpServerTools(GetTimeToolClientTransport      );
    var (weartherMcpClient  , weatherTools   ) = await GetMcpServerTools(GetWeatherToolClientTransport   );
    var (fileSystemMcpClient, fileSystemTools) = await GetMcpServerTools(GetFileSystemToolClientTransport);

    McpClient[]                mcpClients = [timeMcpClient, weartherMcpClient, fileSystemMcpClient];
    IEnumerable<McpClientTool> tools      = [.. timeTools, .. weatherTools, .. fileSystemTools];

    return (mcpClients, tools);

    // MCP サーバー (STDIO) を使うためのクライアント生成
    // - STDIO 経由で MCP サーバー（Time ツール）に接続するためのトランスポート
    // - Command/Arguments を適切に設定して、MCP サーバー プロジェクトを起動
    // - 実際のプロジェクトパスは環境に合わせて更新してください
    static IClientTransport GetTimeToolClientTransport()
        => new StdioClientTransport(new() {
            Name = "time",
            Command = "dotnet",
            // [MCPServer.Con.csprojのフルパス] の部分は、実際のもので置き換えてください
            //Arguments = ["run", "--project", @"[MCPServer.Con.csprojのフルパス]"]
            Arguments = ["run", "--project", @"C:\DropBox\Dropbox\Source\GitHub\Repos\2025.10.AIAgentsSeminarSlide\Shos.AIAgentSample\MCPServer.Con\MCPServer.Con.csproj"]
        });

    // MCP サーバー (HTTP) を使うためのクライアント生成
    static IClientTransport GetWeatherToolClientTransport()
    {
        const string endPoint = "http://localhost:3001/sse";
        return new HttpClientTransport(new HttpClientTransportOptions { Endpoint = new Uri(endPoint) });
    }

    // ファイルシステム ツールを使うためのクライアント生成
    static IClientTransport GetFileSystemToolClientTransport()
        => new StdioClientTransport(new() {
            Name      = "filesystem",
            Command   = "npx"       ,
            // アクセスさせるフォルダーを、仮に "/work" と指定
            Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "/work"]
        });
}

// MCP サーバー (STDIO) のツールを取得
// - STDIO トランスポート経由で McpClient に接続し、ツール一覧を取得して返す
// - 戻り値は (McpClient, IEnumerable<McpClientTool>) で、終了時に McpClient.DisposeAsync() を呼ぶ必要がある
static async Task<(McpClient, IEnumerable<McpClientTool>)> GetMcpServerTools(Func<IClientTransport> getToolClientTransport)
{
    IClientTransport clientTransport = getToolClientTransport();
    McpClient client = await McpClient.CreateAsync(clientTransport);

    IList<McpClientTool> tools = await client.ListToolsAsync();
    foreach (var tool in tools)
        Debug.WriteLine($"{tool.Name} ({tool.Description})");
    return (client, tools);
}
// 新: ここまで

// チャットクライアントの種別
enum ChatClientType
{
    AzureOpenAI,
    Ollama
}
