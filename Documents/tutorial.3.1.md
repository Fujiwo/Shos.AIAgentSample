## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントでの MCP サーバーの利用 (単数)
![AIエージェントでの MCP サーバーの利用 (単数)](./Images/tutorial_banners/tutorial_banner_31.png)

この手順では、AIエージェントを MCP クライアントにし、先に作成した MCP サーバーを利用するようにします。

○ FCAIAgent での作業に戻る

Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」
または、ターミナルで**ソリューションのフォルダー** (例. C:\\Source\\FCAIAgentSample) に移動し、
FCAIAgent プロジェクトにパッケージを追加インストール

```console
cd FCAIAgent
dotnet add package ModelContextProtocol --prerelease
```

○ Program.cs を下記に書き換え

```csharp
// Program.cs
//
// 【概要】
// MCP サーバー (STDIO) を利用するAI エージェントの実行例
// - 指定されたチャットクライアント(Ollama / Azure OpenAI)を作成
// - ChatClientAgent を使った対話を行う
// - ChatClientAgent には、MCP サーバー (STDIO) のツールを渡して利用
//
// 【前提条件】
// - Ollama がインストールされ、http://localhost:11434 で起動していること
// - Ollama でモデル "minimax-m3:cloud" が利用可能であること
// - Azure OpenAI が作成され、エンドポイントと API キーが取得できていること
//
// 【実行方法】
// dotnet run --project FCAIAgent4
//
// 【動作説明】
// 1. 使用するチャットクライアントを作成
// 2. STDIO 経由の MCP サーバーへ接続しツール一覧を取得
// 3. 取得したツールを ChatClientAgent に渡してエージェントを作成
// 4. AgentSession を使った複数ターン対話ループを実行
// 5. 終了時に MCP クライアントを破棄
//
// 【注意】
// - セキュリティ上、実運用では API キーやエンドポイントは環境変数やシークレットストアから読み込むことが望ましい
// - STDIO トランスポートはローカルプロセス間通信向け

using System;
// Microsoft Agent Framework 用
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
// Ollama 用
using OllamaSharp;
// Azure OpenAI のクライアント用
using Azure;
using Azure.AI.OpenAI;
// 新: MCP クライアントとツールを利用するための名前空間
using ModelContextProtocol.Client;
// 新: Debug.WriteLine を使うための名前空間
using System.Diagnostics;

// 使用するチャットクライアント種別
// Ollama を使う場合は My.ChatClientType.Ollama に変更する
const My.ChatClientType chatClientType = My.ChatClientType.AzureOpenAI;
using IChatClient       chatClient     = My.GetChatClient(chatClientType);

// 新: MCP サーバー (STDIO) のツールを取得
var (mcpClient, tools) = await My.GetMcpTools();

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    instructions: My.Instructions,
    // 旧:
    //name        : My.AgentName
    // 新: ここから MCP サーバーのツールを渡す
    name        : My.AgentName,
    description : null,
    // MCP のツール (McpClientTool) は AITool の一種なので、AITool のリストにして渡す
    tools       : tools.Cast<AITool>().ToList()
    // 新: ここまで
);

// 複数ターンに対応するために AgentSession (会話の状態・履歴などを管理) を作成
AgentSession session = await agent.CreateSessionAsync();

// システムメッセージを作成して最初に送信
ChatMessage systemMessage = new(ChatRole.System, My.SystemPrompt);
await My.RunAsync(agent, systemMessage, session);

// 対話の開始と終了方法を表示
Console.WriteLine($"(Interactive chat started. Type '{My.ExitPrompt}' to quit.)\n");

// 対話ループ: ユーザー入力を受け取り exit で終了
for (; ;) {
    var (isValid, userMessage) = My.GetUserMessage();
    // 空行または exit が入力されたら対話を終了
    if (!isValid)
        break;
    await My.RunAsync(agent, userMessage, session);
}

// 新: 終了処理 MCP クライアントを破棄
await mcpClient.DisposeAsync();

// 上記コード中の型や定数、メソッドが自作のものかどうかを判別しやすくするためにクラスに格納
static class My
{
    // エージェント名と指示
    public const string AgentName    = "AIエージェント";
    public const string Instructions = "あなたはAIエージェントです";
    // エージェントのシステムロールに与える文脈的な指示
    public const string SystemPrompt = "あなたはAIエージェントです";
    // 対話を終了するときに入力する文字列
    public const string ExitPrompt   = "exit";

    // Ollama を使う場合のクライアント生成(ローカルの Ollama サーバーに接続)
    static IChatClient GetOllamaClient()
    {
        // Ollama サーバーの URL (既定のポート番号は 11434)
        var uri    = new Uri("http://localhost:11434");
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

    // Azure OpenAI を使う場合のクライアント生成
    static IChatClient GetAzureOpenAIClient()
    {
        // 使用するモデルを指定
        const string deploymentName = "MAI-Thinking-1";
        // エンドポイントと API キーを環境変数から取得し、API キーによる認証情報を作成
        var azureOpenAIEndPoint     = GetEndPoint();
        var openAIApiKey            = GetKey     ();
        var credential              = new AzureKeyCredential(openAIApiKey);

        // Azure OpenAI に接続するクライアントを作成
        var azureOpenAIClient       = new AzureOpenAIClient(new Uri(azureOpenAIEndPoint), credential);
        // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
        IChatClient chatClient      = azureOpenAIClient.GetChatClient(deploymentName)
                                                       .AsIChatClient()
                                                       .AsBuilder()
                                                       .UseFunctionInvocation() // ツール呼び出しを使う
                                                       .Build();
        return chatClient;

        // Azure OpenAI のエンドポイントを環境変数から取得 (未設定の場合は例外)
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

        // Azure OpenAI の API キーを環境変数から取得 (未設定の場合は例外)
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

    // ChatClientType に基づいて適切な IChatClient を返すファクトリ関数
    public static IChatClient GetChatClient(My.ChatClientType chatClientType)
        => chatClientType switch {
            My.ChatClientType.Ollama      => My.GetOllamaClient     (),
            My.ChatClientType.AzureOpenAI => My.GetAzureOpenAIClient(),
            // 未対応の種別の場合は例外
            _ => throw new NotSupportedException($"Chat client type '{chatClientType}' is not supported.")
        };

    // コンソールからユーザー入力を読み取り ChatMessage を返す
    public static (bool isValid, ChatMessage userMessage) GetUserMessage()
    {
        var (isValid, userPrompt) = GetUserPrompt();
        return (isValid, new ChatMessage(ChatRole.User, userPrompt));

        // コンソールから 1 行読み取る (空行または ExitPrompt の場合は isValid が false)
        static (bool isValid, string userPrompt) GetUserPrompt()
        {
            Console.Write("You: ");
            var userPrompt = Console.ReadLine();
            Console.WriteLine();

            return string.IsNullOrWhiteSpace(userPrompt) ||
                   string.Equals(userPrompt.Trim(), ExitPrompt, StringComparison.OrdinalIgnoreCase)
                ? (isValid: false, userPrompt: string.Empty)
                : (isValid: true , userPrompt: userPrompt);
        }
    }

    // エージェントに ChatMessage を投げて応答を取得
    public static async Task RunAsync(AIAgent agent, ChatMessage chatMessage, AgentSession? session = null)
    {
        try {
            // エージェントにメッセージを送信して応答を待つ (session を渡すと、それまでの会話の履歴を踏まえて応答する)
            var response = await agent.RunAsync(chatMessage, session);
            Console.WriteLine($"Agent: {response.Text ?? string.Empty}\n");
        } catch (Exception ex) {
            // 接続できないなどでエージェントの実行に失敗した場合は、エラーメッセージを表示
            Console.WriteLine($"Error running agent: {ex.Message}");
        }
    }

    // 新: ここから MCP サーバーのツール一覧を取得して返す
    // - 戻り値は (McpClient, IEnumerable<McpClientTool>) で、終了時に McpClient.DisposeAsync() を呼ぶ必要がある
    public static async Task<(McpClient, IEnumerable<McpClientTool>)> GetMcpTools()
    {
        IClientTransport clientTransport = GetTimeToolClientTransport();
        // MCP サーバーに接続して MCP クライアントを作成
        McpClient client = await McpClient.CreateAsync(clientTransport);

        // MCP サーバーが提供するツールの一覧を取得
        IList<McpClientTool> tools = await client.ListToolsAsync();
        // 確認用に、ツールの名前と説明をデバッグ出力
        foreach (var tool in tools)
            Debug.WriteLine($"{tool.Name} ({tool.Description})");
        return (client, tools);

        // MCP サーバー (STDIO) を使うためのクライアント生成
        // - STDIO 経由で MCP サーバー(Time ツール)に接続するためのトランスポート
        // - Command/Arguments を適切に設定して、MCP サーバー プロジェクトを起動
        // - 実際のプロジェクトパスは環境に合わせて更新してください
        static IClientTransport GetTimeToolClientTransport()
            => new StdioClientTransport(new() {
                Name      = "time",
                Command   = "dotnet",
                // [McpServer.Con.csprojのフルパス] の部分は、実際のもので置き換えてください
                //Arguments = ["run", "--project", @"[McpServer.Con.csprojのフルパス]"]
                Arguments = ["run", "--project", @"C:\Source\FCAIAgentSample\McpServer.Con\McpServer.Con.csproj"]
            });
    }
    // 新: ここまで

    // チャットクライアントの種別
    public enum ChatClientType
    {
        // Azure OpenAI (Azure 上のモデル)
        AzureOpenAI,
        // Ollama (Ollama サーバー経由のモデル)
        Ollama
    }
}
```

○ 実行
```console
dotnet run
```

- 実行結果の例
```console
Agent: こんにちは。私はAIエージェントです。何を手伝いしましょうか？情報検索、要約、翻訳、文章作成、コードのヘルプ、スケジュール調整や時刻確認（特定のタイムゾーンの現在時刻も取得できます）など対応できます。具体的な内容や希望の形式（箇条書き、詳しい説明、短い回答など）を教えてください。

(Interactive chat started. Type 'exit' to quit.)

You: 現在時刻を教えて

Agent: 現在の時刻は 2025年10月10日 15:04:57（日本標準時 JST / UTC+9）です。他のタイムゾーンや形式に変えますか？

You: exit
```
