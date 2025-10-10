## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ MCP サーバー (STDIO) の作成
![MCP サーバー (STDIO) の作成](./Images/tutorial_banner_21.png)

この手順では、AIエージェントから利用するための STDIO を利用する MCP サーバーを作成します。

Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」で、McpServer.Con プロジェクトを作成

パッケージをインストール
```console
dotnet new console -n McpServer.Con
cd McpServer.Con
dotnet add package Microsoft.Extensions.Hosting
dotnet add package ModelContextProtocol --prerelease
```

Visual Studio の「ソリューション エクスプローラー」でソリューションを右クリックし、「追加」-「既存のプロジェクト」で、McpServer.Con プロジェクトを追加

Program.cs を下記に書き換え

```csharp
// Program.cs

// Model Context Protocol (MCP) サーバーの簡易なサンプル
// - アプリケーションホストを作成し、MCP サーバーを登録して起動
// - 標準入出力（stdio）経由のサーバートランスポートを有効にし、同一アセンブリ内のツールを登録
// - クライアントは標準入出力を通じて MCP プロトコルに従って通信可能

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

// アプリケーションビルダーを作成し、MCP サーバーを登録して実行
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddMcpServer()               // MCP サーバーの基本サービスを登録
       .WithStdioServerTransport()   // 標準入出力経由のトランスポートを有効化
       .WithToolsFromAssembly();     // このアセンブリ内のツールを自動登録

await builder.Build().RunAsync();

// ツール定義: 各メソッドは公開されるツールとして呼び出し可能になる
[McpServerToolType, Description("時刻を取得する")] // このクラスがツールグループであることを示す属性
public static class TimeTools
{
    // 現在の時刻を取得する単純なツール
    [McpServerTool, Description("現在の時刻を取得")] // ツールとして公開し、説明を付与
    public static string GetCurrentTime() => DateTimeOffset.Now.ToString();

    // 指定されたタイムゾーンの現在の時刻を取得するツール
    // 引数 `timezone` はシステムのタイムゾーン ID（例: "Pacific Standard Time" や "Asia/Tokyo"）。
    [McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")] // ツールとして公開し、説明を付与
    public static string GetTimeInTimezone(string timezone)
    {
        try {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone).ToString();
        } catch {
            // 無効なタイムゾーンが指定された場合はエラーメッセージを返す
            return "無効なタイムゾーンが指定されています";
        }
    }
}
```

ビルド確認

```console
dotnet build
```
