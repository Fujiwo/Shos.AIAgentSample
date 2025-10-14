## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントの作成 (LLM利用) - 複数ターンのチャット
![AIエージェントの作成 (LLM利用) - 複数ターンのチャット](./Images/tutorial_banner_13.png)

この手順では、複数ターンの会話を行えるようにエージェントを改良していきます。

Program.cs を下記のようにに書き換え

```csharp
// Program.cs
using System;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
// Azure OpenAI のクライアントを利用するための名前空間
using Azure;
using Azure.AI.OpenAI;

// AI エージェントの実行例
// - 指定されたチャットクライアント（Ollama / Azure OpenAI）を作成
// - ChatClientAgent を使った対話を行う

// エージェント名と指示
const string agentName    = "AIエージェント";
const string instructions = "あなたはAIエージェントです";
// 新: エージェントのシステムロールに与える文脈的な指示
const string systemPrompt = "あなたはAIエージェントです";
// 旧: ユーザーからのプロンプトの例
//const string userPrompt   = "「AIエージェント」とはどのようなものですか?";

// 使用するチャットクライアント種別
const ChatClientType chatClientType = ChatClientType.AzureOpenAI;
IChatClient chatClient = GetChatClient(chatClientType);

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = agentName,
        Instructions = instructions
    }
);

// 旧: ここから
// エージェントを実行して結果を表示する
//AgentRunResponse response = await agent.RunAsync(userPrompt);
//Console.WriteLine(response.Text);
// 旧: ここまで

// 新: ここから
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
// 新: ここまで

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

// チャットクライアントの種別
enum ChatClientType
{
    AzureOpenAI,
    Ollama
}
```

動作確認

```console
dotnet run
```
