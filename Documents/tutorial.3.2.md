## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントでの MCP サーバーの利用 (複数)
![AIエージェントでの MCP サーバーの利用 (複数)](./Images/tutorial_banners/tutorial_banner_32.png)

この手順では、AIエージェントを改良し、複数の MCP サーバーを利用するようにします。

○ Program.cs を下記に書き換え

```csharp
// Program.cs
//
// 【概要】
// 複数の MCP サーバーを利用するAI エージェントの実行例
// - 指定されたチャットクライアント(Ollama / Azure OpenAI)を作成
// - ChatClientAgent を使った対話を行う
// - ChatClientAgent には、複数の MCP サーバーのツールを渡して利用
//
// 【前提条件】
// - Ollama がインストールされ、http://localhost:11434 で起動していること
// - Ollama でモデル "gpt-oss:20b-cloud" が利用可能であること
// - Azure OpenAI が作成され、エンドポイントと API キーが取得できていること
//
// 【実行方法】
// dotnet run --project FCAIAgent
//
// 【動作説明】
// 1. 使用するチャットクライアントを作成
// 2. 複数の MCP サーバー (Time, Weather, FileSystem) へ接続してツールを取得
// 3. 取得したツールを ChatClientAgent に渡してエージェントを作成
// 4. AgentThread を使った複数ターン対話ループを実行
// 5. 終了時に全ての MCP クライアントを破棄
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
// MCP クライアントとツールを利用するための名前空間
using ModelContextProtocol.Client;
// Debug.WriteLine を使うための名前空間
using System.Diagnostics;

// 使用するチャットクライアント種別
const My.ChatClientType chatClientType = My.ChatClientType.AzureOpenAI;
using IChatClient       chatClient     = My.GetChatClient(chatClientType);

// MCP サーバーのツールを取得
// 旧:
//var (mcpClient, tools) = await My.GetMcpTools();
// 新:
var (mcpClients, tools) = await My.GetMcpTools();

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name          = My.AgentName   ,
        Instructions  = My.Instructions,
        // ツールをエージェントに渡す
        ChatOptions = new ChatOptions { Tools = tools.Cast<AITool>().ToList() }
    }
);

// 複数ターンに対応するために AgentThread (会話の状態・履歴などを管理) を作成
AgentThread thread = agent.GetNewThread();

// システムメッセージを作成して最初に送信
ChatMessage systemMessage = new(ChatRole.System, My.SystemPrompt);
await My.RunAsync(agent, systemMessage, thread);

Console.WriteLine($"(Interactive chat started. Type '{My.ExitPrompt}' to quit.)\n");

// 対話ループ: ユーザー入力を受け取り exit で終了
for (; ;) {
    var (isValid, userMessage) = My.GetUserMessage();
    if (!isValid)
        break;
    await My.RunAsync(agent, userMessage, thread);
}

// 終了処理 MCP クライアントを破棄
// 新: ここから
foreach (var mcpClient in mcpClients)
// 新: ここまで
    await mcpClient.DisposeAsync();

// 上記コード中の型や定数、メソッドが自作のものかどうかを判別しやすくするためにクラスに格納
static class My
{
    // エージェント名と指示
    public const string AgentName    = "AIエージェント";
    public const string Instructions = "あなたはAIエージェントです";
    // エージェントのシステムロールに与える文脈的な指示
    public const string SystemPrompt = "あなたはAIエージェントです";
    public const string ExitPrompt   = "exit";

    // Ollama を使う場合のクライアント生成(ローカルの Ollama サーバーに接続)
    static IChatClient GetOllamaClient()
    {
        var uri    = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);
        // 使用するモデルを指定
        // クラウドベースのモデルを使用(実行速度の向上のため)
        // ローカル LLM を使用する場合は "gemma3:latest" などに変更してください
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
        // 使用するモデルを指定
        const string deploymentName = "gpt-5-mini";
        var azureOpenAIEndPoint     = GetEndPoint();
        var openAIApiKey            = GetKey     ();
        var credential              = new AzureKeyCredential(openAIApiKey);

        var azureOpenAIClient       = new AzureOpenAIClient(new Uri(azureOpenAIEndPoint), credential);
        // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
        IChatClient chatClient      = azureOpenAIClient.GetChatClient(deploymentName)
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
            // 例: 1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef
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
            //例: https://your-resource-name.openai.azure.com/
            return @"[Azure OpenAI の APIキー]";
        }
    }

    // ChatClientType に基づいて適切な IChatClient を返すファクトリ関数
    public static IChatClient GetChatClient(My.ChatClientType chatClientType)
        => chatClientType switch {
            My.ChatClientType.Ollama      => My.GetOllamaClient     (),
            My.ChatClientType.AzureOpenAI => My.GetAzureOpenAIClient(),
            _ => throw new NotSupportedException($"Chat client type '{chatClientType}' is not supported.")
        };

    // コンソールからユーザー入力を読み取り ChatMessage を返す
    public static (bool isValid, ChatMessage userMessage) GetUserMessage()
    {
        var (isValid, userPrompt) = GetUserPrompt();
        return (isValid, new ChatMessage(ChatRole.User, userPrompt));

        static (bool isValid, string userPrompt) GetUserPrompt()
        {
            Console.Write("You: ");
            var userPrompt = Console.ReadLine();
            Console.WriteLine();

            return string.IsNullOrWhiteSpace(userPrompt) ||
                   string.Equals(userPrompt.Trim(), ExitPrompt, StringComparison.OrdinalIgnoreCase)
                ? (isValid: false, userPrompt: string.Empty)
                : (isValid: true , userPrompt: userPrompt! );
        }
    }

    // エージェントに ChatMessage を投げて応答を取得
    public static async Task RunAsync(AIAgent agent, ChatMessage chatMessage, AgentThread? thread = null)
    {
        try {
            var response = await agent.RunAsync(chatMessage, thread);
            Console.WriteLine($"Agent: {response.Text ?? string.Empty}\n");
        } catch (Exception ex) {
            Console.WriteLine($"Error running agent: {ex.Message}");
        }
    }

    // 旧: ここから
    //// MCP サーバーのツール一覧を取得して返す
    //// - 戻り値は (McpClient, IEnumerable<McpClientTool>) で、終了時に McpClient.DisposeAsync() を呼ぶ必要がある
    //public static async Task<(McpClient, IEnumerable<McpClientTool>)> GetMcpTools()
    //{
    //    IClientTransport clientTransport = GetTimeToolClientTransport();
    //    McpClient client = await McpClient.CreateAsync(clientTransport);

    //    IList<McpClientTool> tools = await client.ListToolsAsync();
    //    foreach (var tool in tools)
    //        Debug.WriteLine($"{tool.Name} ({tool.Description})");
    //    return (client, tools);

    //    // MCP サーバー (STDIO) を使うためのクライアント生成
    //    // - STDIO 経由で MCP サーバー(Time ツール)に接続するためのトランスポート
    //    // - Command/Arguments を適切に設定して、MCP サーバー プロジェクトを起動
    //    // - 実際のプロジェクトパスは環境に合わせて更新してください
    //    static IClientTransport GetTimeToolClientTransport()
    //        => new StdioClientTransport(new() {
    //            Name      = "time",
    //            Command   = "dotnet",
    //            // [MCPServer.Con.csprojのフルパス] の部分は、実際のもので置き換えてください
    //            Arguments = ["run", "--project", @"[MCPServer.Con.csprojのフルパス]"]
    //            //Arguments = ["run", "--project", @"C:\Source\FCAIAgentSample\MCPServer.Con\MCPServer.Con.csproj"]
    //        });
    //}
    // 旧: ここまで

    // 新: ここから
    // 複数の MCP サーバーのツールを取得
    // - 複数の McpClient に接続し、ツール一覧を取得して返す
    // - 戻り値は (IEnumerable<McpClient>, IEnumerable<McpClientTool>) で、終了時にすべての McpClient について DisposeAsync() を呼ぶ必要がある
    public static async Task<(IEnumerable<McpClient>, IEnumerable<McpClientTool>)> GetMcpTools()
    {
        var (timeMcpClient      , timeTools      ) = await GetMcpTools(GetTimeToolClientTransport      );
        var (weartherMcpClient  , weatherTools   ) = await GetMcpTools(GetWeatherToolClientTransport   );
        var (fileSystemMcpClient, fileSystemTools) = await GetMcpTools(GetFileSystemToolClientTransport);

        McpClient[] mcpClients = [timeMcpClient, weartherMcpClient, fileSystemMcpClient];
        IEnumerable<McpClientTool> tools = [.. timeTools, .. weatherTools, .. fileSystemTools];

        return (mcpClients, tools);

        // MCP サーバー (STDIO) を使うためのクライアント生成
        // - STDIO 経由で MCP サーバー(Time ツール)に接続するためのトランスポート
        // - Command/Arguments を適切に設定して、MCP サーバー プロジェクトを起動
        // - 実際のプロジェクトパスは環境に合わせて更新してください
        static IClientTransport GetTimeToolClientTransport()
            => new StdioClientTransport(new() {
                Name = "time",
                Command = "dotnet",
                // [MCPServer.Con.csprojのフルパス] の部分は、実際のもので置き換えてください
                Arguments = ["run", "--project", @"[MCPServer.Con.csprojのフルパス]"]
                //Arguments = ["run", "--project", @"C:\Source\FCAIAgentSample\MCPServer.Con\MCPServer.Con.csproj"]
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
                Command   = "npx",
                // アクセスさせるフォルダーを、仮に "/work" と指定
                Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "/work"]
            });

        // MCP サーバー (STDIO) のツール一覧を取得して返す
        // - 戻り値は (McpClient, IEnumerable<McpClientTool>) で、終了時に McpClient.DisposeAsync() を呼ぶ必要がある
        static async Task<(McpClient, IEnumerable<McpClientTool>)> GetMcpTools(Func<IClientTransport> getToolClientTransport)
        {
            IClientTransport clientTransport = getToolClientTransport();
            McpClient client = await McpClient.CreateAsync(clientTransport);

            IList<McpClientTool> tools = await client.ListToolsAsync();
            foreach (var tool in tools)
                Debug.WriteLine($"{tool.Name} ({tool.Description})");
            return (client, tools);
        }
    }
    // 新: ここまで

    // チャットクライアントの種別
    public enum ChatClientType
    {
        AzureOpenAI,
        Ollama
    }
}
```

○ 実行

- 予め "\\work" フォルダーを作成し、中にファイルを入れておく (sample1\.txt, sample2\.txt など)

- 予め McpServer\.Sse を実行しておく
  
Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」
または、ターミナルで**ソリューションのフォルダー** (例. C:\\Source\\FCAIAgentSample) に移動し、
```console
cd McpServer.Sse
dotnet run
```

- FCAIAgent の実行
```console
cd FCAIAgent
dotnet run
```

- 実行結果の例

・FCAIAgent (AIエージェント側)
```console
Agent: こんにちは。AIエージェントです。何をお手伝いしましょうか？

例：
- 情報や調べ物（要約や比較）
- 文章作成・校正（メール、レポート、企画書など）
- プログラムの相談やコード修正
- スケジュールや旅行計画の提案
- ファイル操作やリポジトリの確認（指示があれば）

やってほしいことを教えてください。

(Interactive chat started. Type 'exit' to quit.)

You: 福井県の天気は?

Agent: 福井県の天気予報は「雪」です。

もっと詳しい情報（市区町村ごとの予報、気温、降水確率、時間ごとの予報、明日の予報、注意報／警報など）が欲しい場合は、どの市の分がよいか／どの時間帯の情報が必要か教えてください。道路の凍結や視界不良に注意してください。

You: ファイル一覧

Agent: C:\work のファイル一覧です：

- sample1.txt
- sample2.txt

どのファイルを開きますか？（例：内容を表示／メタ情報取得／複数ファイルを同時に読む／ディレクトリツリー表示）

You:
```

・McpServer\.Sse (MCPサーバー側)
```console
info: ModelContextProtocol.Server.McpServer[570385771]
      Server (McpServer.Sse 1.0.0.0) method 'initialize' request handler called.
info: ModelContextProtocol.Server.McpServer[1867955179]
      Server (McpServer.Sse 1.0.0.0), Client (FCAIAgent5 1.0.0.0) method 'initialize' request handler completed.
info: ModelContextProtocol.Server.McpServer[570385771]
      Server (McpServer.Sse 1.0.0.0), Client (FCAIAgent5 1.0.0.0) method 'tools/list' request handler called.
info: ModelContextProtocol.Server.McpServer[1867955179]
      Server (McpServer.Sse 1.0.0.0), Client (FCAIAgent5 1.0.0.0) method 'tools/list' request handler completed.
info: ModelContextProtocol.Server.McpServer[570385771]
      Server (McpServer.Sse 1.0.0.0), Client (FCAIAgent5 1.0.0.0) method 'tools/call' request handler called.
info: ModelContextProtocol.Server.McpServer[1867955179]
      Server (McpServer.Sse 1.0.0.0), Client (FCAIAgent5 1.0.0.0) method 'tools/call' request handler completed.
```