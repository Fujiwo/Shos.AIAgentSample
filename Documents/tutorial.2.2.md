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
cd McpServer.Sse
```

Visual Studio の場合、Visual Studio の「ソリューション エクスプローラー」でソリューションを右クリックし、「追加」-「既存のプロジェクト」で、McpServer.Sse プロジェクトを追加

○ パッケージをインストール
```console
dotnet add package ModelContextProtocol.AspNetCore --prerelease
```

○ Program.cs を下記に書き換え

```csharp
// Program.cs

// Model Context Protocol (MCP) サーバーの簡易なサンプル
// - WebApplication を使って MCP サーバーをホスト
// - HTTP トランスポートを有効にすることで、SSE（Server-Sent Events）を使ったリアルタイム通信をサポート
// - 同一アセンブリ内に定義された McpServer ツールを登録
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
    [McpServerTool, Description("指定した場所の天気を予報します")] // ツールとして公開し、説明を付与
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
