## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントの作成 (LLM利用) - Azure OpenAIの利用
![AIエージェントの作成 (LLM利用) - Azure OpenAIの利用](./Images/tutorial_banner_12.png)

この手順では、LLM として Azure OpenAI も利用できるようにエージェントを改良していきます。

パッケージを追加でインストール

```console
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
```

Program.cs を下記のようにに書き換え

```csharp
// Program.cs
using System;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OllamaSharp;
// 新: ここから Azure OpenAI のクライアントを利用するための名前空間
using Azure;
using Azure.AI.OpenAI;
// 新: ここまで

// AI エージェントの実行例
// - 指定されたチャットクライアント（Ollama / Azure OpenAI）を作成
// - ChatClientAgent を作成して、簡単なプロンプトを投げる

// エージェント名と指示
const string agentName    = "AIエージェント";
const string instructions = "あなたはAIエージェントです";
// ユーザーからのプロンプトの例
const string userPrompt   = "「AIエージェント」とはどのようなものですか?";

// 旧: IChatClient chatClient = GetOllamaClient();
// 新: ここから
// 使用するチャットクライアント種別
const ChatClientType chatClientType = ChatClientType.AzureOpenAI;
IChatClient chatClient = GetChatClient(chatClientType);
// 新: ここまで

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = agentName,
        Instructions = instructions
    }
);

// エージェントを実行して結果を表示する
AgentRunResponse response = await agent.RunAsync(userPrompt);
Console.WriteLine(response.Text);

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

// 新: ここから Azure OpenAI を使う場合のクライアント生成
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
// 新: ここまで
```

動作確認

```console
dotnet run
```
