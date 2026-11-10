// Model Context Protocol (MCP) サーバーの簡易なサンプル
// - WebApplication を使って MCP サーバーをホスト
// - HTTP トランスポート (Streamable HTTP) を有効にして、HTTP 経由での通信をサポート
// - 同一アセンブリ内に定義された Mcp ツールを登録
//
// 1. このプログラムを実行するとローカルホストの http://localhost:3001 でサーバーが起動
// 2. クライアントは MCP のプロトコル (Streamable HTTP) に従って http://localhost:3001/ に接続する
//    ※ 旧来の SSE エンドポイント (/sse) は既定では無効。必要な場合は WithHttpTransport(options => options.EnableLegacySse = true) とする
// 3. ツールは `[McpServerToolType]` クラス内の `[McpServerTool]` 属性が付与されたメソッドとして公開される
// 4. 各メソッドには `Description` 属性を付与しておくと、ツールの説明がクライアントに提供される
//
// セキュリティと運用に関する注意:
// - 公開環境では TLS、認証、認可、CORS 設定などを検討する必要がある

using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// MCP サーバーをサービスに追加し、HTTP トランスポート (Streamable HTTP) とアセンブリ内のツールを登録
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
