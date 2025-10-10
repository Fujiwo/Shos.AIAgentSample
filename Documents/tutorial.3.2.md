## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントでの MCP サーバーの利用 (複数)

Program.cs を下記に書き換え

```csharp
// Program.cs
using System;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
// Azure OpenAI のクライアントを利用するための名前空間
using Azure;
using Azure.AI.OpenAI;
// 新: MCP クライアントとツールを利用するための名前空間
using ModelContextProtocol.Client;
// 新: Debug.WriteLine を使うための名前空間
using System.Diagnostics;

// MCP サーバー (STDIO) を利用するAI エージェントの実行例
// - 指定されたチャットクライアント（Ollama / Azure OpenAI）を作成
// - ChatClientAgent を使った対話を行う
// - ChatClientAgent には、MCP サーバー (STDIO) のツールを渡して利用
//
// 主な流れ:
// 1) 使用するチャットクライアントを作成
// 2) STDIO 経由の MCP サーバーへ接続しツール一覧を取得
// 3) 取得したツールを ChatClientAgent に渡してエージェントを作成
// 4) AgentThread を使った複数ターン対話ループを実行
// 5) 終了時に MCP クライアントを破棄
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

// 新: MCP サーバー (STDIO) のツールを取得
var (mcpClient, tools) = await GetMcpServerTools();

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = agentName,
        Instructions = instructions
        // 新: ツールをエージェントに渡す
        , ChatOptions = new ChatOptions { Tools = tools.Cast<AITool>().ToList() }
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

// 新: 終了処理 MCP クライアントを破棄
await mcpClient.DisposeAsync();

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
    ollama.SelectedModel = "gpt-oss:20b-cloud";

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
        //const string AzureOpenAIEndpointEnvironmentVariable = "AZURE_OPENAI_ENDPOINT";
        //var azureOpenAIEndPoint = Environment.GetEnvironmentVariable(AzureOpenAIEndpointEnvironmentVariable);
        //if (string.IsNullOrEmpty(azureOpenAIEndPoint))
        //    throw new InvalidOperationException($"Please set the {AzureOpenAIEndpointEnvironmentVariable} environment variable.");
        //return azureOpenAIEndPoint;

        // 上記のように、セキュリティ上 Azure OpenAI のエンドポイントは環境変数から取得するのが望ましいが、ここではハードコードする
        // [Azure OpenAI のエンドポイント] の部分は、実際のもので置き換えてください
        return @"[Azure OpenAI のエンドポイント]";
    }

    static string GetKey()
    {
        //const string AzureOpenAIApiKeyEnvironmentVariable = "AZURE_OPENAI_API_KEY";
        //var openAIApiKey = Environment.GetEnvironmentVariable(AzureOpenAIApiKeyEnvironmentVariable);
        //if (string.IsNullOrEmpty(openAIApiKey))
        //    throw new InvalidOperationException($"Please set the {AzureOpenAIApiKeyEnvironmentVariable} environment variable.");
        //return openAIApiKey!;

        // 上記のように、セキュリティ上 Azure OpenAI の APIキーは環境変数から取得するのが望ましいが、ここではハードコードする
        // [Azure OpenAI の APIキー] の部分は、実際のもので置き換えてください
        return @"[Azure OpenAI の APIキー]";
    }
}

// ChatClientType に基づいて適切な IChatClient を返すファクトリ関数
static IChatClient GetChatClient(ChatClientType chatClientType)
    => chatClientType switch {
        ChatClientType.Ollama      => GetOllamaClient     (),
        ChatClientType.AzureOpenAI => GetAzureOpenAIClient(),
        _ => throw new NotSupportedException($"Chat client type '{chatClientType}' is not supported.")
    };

// 新: ここから
// MCP サーバー (STDIO) のツールを取得
// - STDIO トランスポート経由で McpClient に接続し、ツール一覧を取得して返す
// - 戻り値は (McpClient, IEnumerable<McpClientTool>) で、終了時に McpClient.DisposeAsync() を呼ぶ必要がある
static async Task<(McpClient, IEnumerable<McpClientTool>)> GetMcpServerTools()
{
    IClientTransport clientTransport = GetTimeToolClientTransport();
    McpClient client = await McpClient.CreateAsync(clientTransport);

    IList<McpClientTool> tools = await client.ListToolsAsync();
    foreach (var tool in tools)
        Debug.WriteLine($"{tool.Name} ({tool.Description})");
    return (client, tools);
}

// MCP サーバー (STDIO) を使うためのクライアント生成
// - STDIO 経由で MCP サーバー（Time ツール）に接続するためのトランスポート
// - Command/Arguments を適切に設定して、MCP サーバー プロジェクトを起動
static IClientTransport GetTimeToolClientTransport()
    => new StdioClientTransport(new() {
        Name      = "time"  ,
        Command   = "dotnet",
        // [MCPServer.Con.csprojのフルパス] の部分は、実際のもので置き換えてください
        Arguments = ["run", "--project", @"[MCPServer.Con.csprojのフルパス]"]
    });
// 新: ここまで

// チャットクライアントの種別
enum ChatClientType
{
    AzureOpenAI,
    Ollama
}
```

実行
```console
dotnet run
```

実行結果の例
```console
Agent: こんにちは。私はAIエージェントです。何を手伝いしましょうか？情報検索、要約、翻訳、文章作成、コードのヘルプ、スケジュール調整や時刻確認（特定のタイムゾーンの現在時刻も取得できます）など対応できます。具体的な内容や希望の形式（箇条書き、詳しい説明、短い回答など）を教えてください。

(Interactive chat started. Type 'exit' to quit.)

You: 現在時刻を教えて

Agent: 現在の時刻は 2025年10月10日 15:04:57（日本標準時 JST / UTC+9）です。他のタイムゾーンや形式に変えますか？

You: exit
```