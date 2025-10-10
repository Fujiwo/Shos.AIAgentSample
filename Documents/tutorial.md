
## チュートリアル

### ■ 準備

#### 1\. \.NET

- 1\.1 \.NET のバージョンの確認

```console
dotnet --version
```
  9\.0以上であればOK

- 1\.2 (9\.0未満の場合) \.NET9\.0 のダウンロード

![Download \.NET](./Images/download_dotnet.png)

- 1\.3 (9\.0未満の場合) \.NET9\.0 のインストール

![Install \.NET](./Images/install_dotnet.png)

- 1\.4 \.NET のバージョンの再確認

```console
dotnet --version
```

#### 2\. Node.js の Windows 版をインストール

- 2\.1 Node.js のダウンロード

  - [Node\.js — Node\.js®をダウンロードする](https://nodejs.org/ja/download)

![Download Node.js](./images/download_nodejs.png)

- 2\.2 Node.js のインストール

![Node\.js のインストール](./Images/nodejs_installer.png)

- 2\.2 確認

```console
node -v
```

```console
npm -v
```

- 2\.3 npx のインストール

```console
npm install -g npx
```

- 2\.4 確認

```console
npx -v
```

#### 3\. Ollama の Windows 版をインストール

- 3\.1 Ollama の Windows 版をダウンロード

  - [Ollama](https://www.ollama.com)

![Download Ollama](./Images/download_ollama.png)

- 3\.2 Ollama をインストール
![Install Ollama](./Images/install_ollama.png)

- 3\.3 起動して、サインアップ

![Sign up Ollama](./Images/signup_ollama.png)

- 3\.4 動作確認

![Ollama](./Images/ollama.png)


### ■ Azure OpenAI の準備

#### 1\. Visual Studio Subscription の Azure 特典のアクティブ化

- [Visual Studio Subscriptions Portal](https://my.visualstudio.com)

![Visual Studio Subscription 特典](./Images/vs_azure.png)

#### 2\. Azure OpenAI で LLM を作成

- 2\.1 [Microsoft Azure Portal](https://portal.azure.com) へ行き、「リソースの作成」

![リソースの作成 | Azure Portal](./Images/azure_portal.png)

- 2\.2 「Azure OpenAI」で検索し、「Azure OpenAI」をクリック

![Azure OpenAI | リソースの作成](./Images/azure_create_resource.png)

- 2\.3 「Azure OpenAI」を作成

![Azure OpenAI の作成](./Images/azure_openai1.png)

リソース グループは新規作成する

![リソース グループの新規作成 | Azure OpenAI の作成](./Images/azure_openai2.png)

下記のように入力し、「次へ」を3回クリック
※ 「202510azureopenai」の部分は、別のユニークな文字列とする
※ この文字列はメモ

![Azure OpenAI の作成](./Images/azure_openai3.png)

「作成」

![Azure OpenAI の作成](./Images/azure_openai4.png)

デプロイが終わったら、「リソースへ移動」

![Azure OpenAI の作成](./Images/azure_openai5.png)

「Azure AI Foundry ポータルへ移動」

![Azure AI Foundry ポータルへ移動](./Images/azure_openai6.png)

「APIキー」と「Azure OpenAI エンドポイント」をコピーしてメモ

![Azure AI Foundry ポータルへ移動](./Images/azure_openai7.png)


「チャット」を選び、「デプロイの作成」

![チャット](./Images/azure_openai8.png)

「gpt-5-mini」を選び、「確認」

![デプロイの作成](./Images/azure_openai9.png)

「デプロイ」

![デプロイ](./Images/azure_openai10.png)


### ■ AIエージェントの作成

#### 1\. ローカルLLMの利用

1.1 C#/.NET コンソール アプリケーションを作成

Visual Studio で空のソリューションを作成し、
メニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」

```console
dotnet new console -n FCAIAgent
cd FCAIAgent
```

Microsoft Agent Framework パッケージをインストール

```console
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package OllamaSharp
```

Visual Studio の「ソリューション エクスプローラー」でソリューションを右クリックし、「追加」-「既存のプロジェクト」で、FCAIAgent プロジェクトを追加

Program.cs を下記に書き換え

```csharp
// Program.cs
using System;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OllamaSharp;

// AI エージェントの実行例
// - 指定されたチャットクライアント（Ollama）を作成
// - ChatClientAgent を作成して、簡単なプロンプトを投げる

const string agentName = "AIエージェント";
const string instructions = "あなたはAIエージェントです";
const string userPrompt = "「AIエージェント」とはどのようなものですか?";

// Ollama を使うためのクライアント生成
IChatClient chatClient = GetOllamaClient();

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
    ollama.SelectedModel = "gpt-oss:20b-cloud";

    // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
    IChatClient chatClient = ollama;
    chatClient = chatClient.AsBuilder()
                           .UseFunctionInvocation() // ツール呼び出しを使う
                           .Build();
    return chatClient;
}
```

Ollama が起動していることを確認して、動作確認

```console
dotnet run
```

#### 2\. Azure OpenAIの利用

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

const string agentName    = "AIエージェント";
const string instructions = "あなたはAIエージェントです";
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
    ollama.SelectedModel = "gpt-oss:20b-cloud";

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
        return @"[Azure OpenAI のエンドポイント";
    }

    static string GetKey()
    {
        //const string AzureOpenAIApiKeyEnvironmentVariable = "AZURE_OPENAI_API_KEY";
        //var openAIApiKey = Environment.GetEnvironmentVariable(AzureOpenAIApiKeyEnvironmentVariable);
        //if (string.IsNullOrEmpty(openAIApiKey))
        //    throw new InvalidOperationException($"Please set the {AzureOpenAIApiKeyEnvironmentVariable} environment variable.");
        //return openAIApiKey!;

        // 上記のように、セキュリティ上 Azure OpenAI の APIキーは環境変数から取得するのが望ましいが、ここではハードコードする
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


1.2 Semantic Kernel のパッケージをインストール

```console
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Core
dotnet add package Microsoft.SemanticKernel.Agents.Core --prerelease
dotnet add package Microsoft.SemanticKernel.Connectors.Ollama --prerelease
```

1.3 Program.cs を書き換え

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Kernel.CreateBuilder();
// 👇🏼 Using "gpt-oss:20b-cloud" with Ollama
const string modelName = "gpt-oss:20b-cloud";
builder.AddOllamaChatCompletion(modelName, new Uri("http://localhost:11434"));

var kernel = builder.Build();

ChatCompletionAgent agent = new() { // 👈🏼 Definition of the agent
    Instructions = "LLM (Large Language Model) に関する質問に答えてください。",
    Name = "Agent",
    Kernel = kernel
};

ChatHistory chat = [
    new ChatMessageContent(AuthorRole.User,
                           "LLM とはどのようなものですか?")
];

await foreach (var response in agent.InvokeAsync(chat)) {
    chat.Add(response);
    Console.WriteLine(response.Message.Content);
}
```

1.4 実行

```console
dotnet run
```

### ■ MCP サーバー (STDIO) の作成

Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」で、McpServer.Con プロジェクトを作成

```console
dotnet new console -n McpServer.Con
cd McpServer.Con
dotnet add package Microsoft.Extensions.Hosting
dotnet add package ModelContextProtocol --prerelease
```

Visual Studio の「ソリューション エクスプローラー」でソリューションを右クリックし、「追加」-「既存のプロジェクト」で、McpServer.Con プロジェクトを追加

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class TimeTools
{
    [McpServerTool, Description("現在の時刻を取得")]
    public static string GetCurrentTime() => DateTimeOffset.Now.ToString();

    [McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
    public static string GetTimeInTimezone(string timezone)
    {
        try {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone).ToString();
        } catch {
            return "無効なタイムゾーンが指定されています";
        }
    }
}
```

```console
dotnet build
```

### ■ MCP サーバー (SSE) の作成

Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」で、McpServer.Sse プロジェクトを作成

```console
dotnet new web -n McpServer.Sse
cd McpServer.Sse
dotnet add package ModelContextProtocol.AspNetCore --prerelease
```

Visual Studio の「ソリューション エクスプローラー」でソリューションを右クリックし、「追加」-「既存のプロジェクト」で、McpServer.Sse プロジェクトを追加

```csharp
// Program.cs
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();
var application = builder.Build();

application.MapMcp();
application.Run("http://localhost:3001");

[McpServerToolType, Description("天気を予報する")]
public static class WeatherForecastTool
{
    [McpServerTool, Description("指定した場所の天気を予報します")]
    public static string GetWeatherForecast(
        [Description("天気を予報したい都道府県名")]
        string location) => location switch {
            "北海道" => "晴れ",
            "東京都" => "曇り",
            "石川県" => "雨",
            "福井県" => "雪",
            _       => "晴か曇りか雨か雪か霙か霰か何か"
        };
}
```

```console
dotnet run
```
