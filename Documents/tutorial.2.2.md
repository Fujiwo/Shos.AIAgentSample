## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ MCP サーバー (SSE) の作成
![MCP サーバー (SSE) の作成](./Images/tutorial_banner_22.png)

この手順では、AIエージェントから利用するための HTTP を利用する MCP サーバーを作成します。

○ 新たなプロジェクト \"McpServer.Sse\" の作成

Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」
または、ターミナルで**ソリューションのフォルダー** (例. C:\\Source\\FCAIAgentSample) に移動し、
McpServer.Sse プロジェクトを作成
```console
dotnet new web -n McpServer.Sse
dotnet sln add ./McpServer.Sse/McpServer.Sse.csproj
cd McpServer.Sse
```

- 実行結果
```console
C:\Source\FCAIAgentSample>dotnet new web -n McpServer.Sse
テンプレート ""ASP.NET Core (空)" が正常に作成されました。

作成後の操作を処理しています...
C:\Source\FCAIAgentSample\McpServer.Sse\McpServer.Sse.csproj を復元しています:
正常に復元されました。

C:\Source\FCAIAgentSample>dotnet sln add ./McpServer.Sse/McpServer.Sse.csproj
プロジェクト `McpServer.Sse\McpServer.Sse.csproj` をソリューションに追加しました。

C:\Source\FCAIAgentSample>cd McpServer.Sse

C:\Source\FCAIAgentSample\McpServer.Sse>
```

○ パッケージをインストール
```console
dotnet add package ModelContextProtocol.AspNetCore --prerelease
```

○ Program.cs を下記に書き換え

```csharp
// Program.cs
//
// Model Context Protocol (MCP) サーバーの簡易なサンプル
// - WebApplication を使って MCP サーバーをホスト
// - HTTP トランスポートを有効にすることで、SSE（Server-Sent Events）を使ったリアルタイム通信をサポート
// - 同一アセンブリ内に定義された Mcp ツールを登録
//
// 1. このプログラムを実行するとローカルホストの http://localhost:3001 でサーバーが起動
// 2. クライアントは MCP のプロトコルに従って HTTP エンドポイント経由で接続し、SSE でリアルタイム通信
// 3. ツールは `[McpServerToolType]` クラス内の `[McpServerTool]` 属性が付与されたメソッドとして公開される
// 4. 各メソッドには `Description` 属性を付与しておくと、ツールの説明がクライアントに提供される
//
// セキュリティと運用に関する注意:
// - 公開環境では TLS、認証、認可、CORS 設定などを検討する必要がある

using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// MCP サーバーをサービスに追加し、HTTP トランスポート（SSE 対応）とアセンブリ内のツールを登録
builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

var application = builder.Build();

// MCP のルーティングをマップしてサーバーを起動
application.MapMcp();
application.Run("http://localhost:3001");

// ツール定義: 各メソッドは公開されるツールとして呼び出し可能になる
[McpServerToolType, Description("天気を予報する")] // このクラスがツールグループであることを示す属性
public static class WeatherForecastTool
{
    // 指定した場所の天気を予報します。
    // 引数 `location` には都道府県名などの文字列を渡します。
    [McpServerTool, Description("指定した場所の天気予報を取得")] // ツールとして公開し、説明を付与
    public static string GetWeatherForecast(
        [Description("天気を予報したい都道府県名")] // 引数に説明を付与
        string location) => location switch {
            "北海道" => "晴れ",
            "東京都" => "曇り",
            "石川県" => "雨"  ,
            "福井県" => "雪"  ,
            _       => "晴か曇りか雨か雪か霙か霰か何か"
        };
}
```

○ ビルド確認

```console
dotnet run
```
