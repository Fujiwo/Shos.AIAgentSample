
## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

0\. [準備](./tutorial.00.md)
1\. [AIエージェントの作成 (LLM利用まで)](./tutorial.00.md)




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
