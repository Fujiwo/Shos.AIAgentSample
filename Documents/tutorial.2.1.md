## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ MCP サーバー (STDIO) の作成
![MCP サーバー (STDIO) の作成](./Images/tutorial_banners/tutorial_banner_21.png)

この手順では、AIエージェントから利用するための STDIO を利用する MCP サーバーを作成します。

○ 新たなプロジェクト \"McpServer.Con\" の作成

Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」
または、ターミナルで**ソリューションのフォルダー** (例. C:\\Source\\FCAIAgentSample) に移動し、
McpServer.Con プロジェクトを作成

```console
dotnet new console -n McpServer.Con
cd McpServer.Con
```

- 実行結果
```console
C:\Source\FCAIAgentSample>dotnet new console -n McpServer.Con
テンプレート "コンソール アプリ" が正常に作成されました。

作成後の操作を処理しています...
C:\Source\FCAIAgentSample\McpServer.Con\McpServer.Con.csproj を復元しています:
正常に復元されました。

C:\Source\FCAIAgentSample>dotnet sln add ./McpServer.Con/McpServer.Con.csproj
プロジェクト `McpServer.Con\McpServer.Con.csproj` をソリューションに追加しました。

C:\Source\FCAIAgentSample>cd McpServer.Con

C:\Source\FCAIAgentSample\McpServer.Con>
```

○ パッケージをインストール
```console
dotnet add package Microsoft.Extensions.Hosting
dotnet add package ModelContextProtocol --prerelease
```

○ Program.cs を下記に書き換え

```csharp
// Program.cs
//
// Model Context Protocol (MCP) サーバーの簡易なサンプル
// - アプリケーションホストを作成し、MCP サーバーを登録して起動
// - 標準入出力（stdio）経由のサーバートランスポートを有効にし、同一アセンブリ内のツールを登録
// - クライアントは標準入出力を通じて MCP プロトコルに従って通信可能

// 汎用ホストと依存性注入 (DI) 用
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// MCP サーバー用
using ModelContextProtocol.Server;
// Description 属性用
using System.ComponentModel;

// アプリケーションビルダーを作成し、MCP サーバーを登録して実行
// 標準出力は MCP の通信に使うため、ログなどを出力しない空のビルダーを使う
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddMcpServer()               // MCP サーバーの基本サービスを登録
       .WithStdioServerTransport()   // 標準入出力経由のトランスポートを有効化
       .WithToolsFromAssembly();     // このアセンブリ内のツールを自動登録

// ホストをビルドして実行 (クライアントとの接続が終わるまで待機)
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
    public static string GetTimeInTimezone(
        [Description("タイムゾーン ID (例: \"Asia/Tokyo\" や \"Tokyo Standard Time\")")] // 引数に説明を付与
        string timezone)
    {
        try {
            // タイムゾーン ID からタイムゾーンを取得し、現在時刻をそのタイムゾーンの時刻に変換
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone).ToString();
        } catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) {
            // 無効なタイムゾーンが指定された場合はエラーメッセージを返す
            return "無効なタイムゾーンが指定されています";
        }
    }
}
```

○ ビルド確認

```console
dotnet build
```
