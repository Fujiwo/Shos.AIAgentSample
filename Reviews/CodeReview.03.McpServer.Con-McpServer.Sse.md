# McpServer.Con / McpServer.Sse コードレビュー #01

レビュー日: 2025-10-20  
対象ファイル: 
- `McpServer.Con/Program.cs`
- `McpServer.Sse/Program.cs`

## 概要

McpServer.Con と McpServer.Sse は、Model Context Protocol (MCP) サーバーの実装サンプルです。McpServer.Con は標準入出力（stdio）経由のトランスポートを使用し、McpServer.Sse は HTTP/SSE（Server-Sent Events）を使用したリアルタイム通信をサポートします。両プロジェクトとも、属性ベースでツールを定義し、クライアントから呼び出し可能にする簡潔な実装となっています。

---

## 1. バグ・潜在的な問題点

### 1.1 McpServer.Sse - 文字エンコーディングの問題（重大）

**問題点:**
McpServer.Sse/Program.cs が Shift-JIS エンコーディングで保存されているため、ビルド時にコンパイルエラーが発生する可能性があります。実際に `dotnet build` を実行した際に以下のエラーが確認されました：

```
error CS1009: Unrecognized escape sequence
```

C# ソースファイルは UTF-8（BOM なし）で保存することが推奨されます。現在のファイルは Shift-JIS で保存されているため、日本語の文字列リテラル内でバイトシーケンスがエスケープシーケンスとして誤認識される問題があります。

**修正案:**
ファイルを UTF-8（BOM なし）で再保存してください。Visual Studio、Visual Studio Code、または任意のテキストエディタで以下の手順を実行してください。

1. ファイルを開く
2. エンコーディングを UTF-8（BOM なし）に変更
3. 保存

または、次のコマンドを使用して変換できます：
```bash
iconv -f SHIFT_JIS -t UTF-8 Program.cs > Program_utf8.cs
mv Program_utf8.cs Program.cs
```

### 1.2 McpServer.Con - BOM（Byte Order Mark）の存在

**問題点:**
McpServer.Con/Program.cs の先頭に BOM（`﻿`）が含まれています（行 1）。

**修正案:**
UTF-8 BOM なしでファイルを保存してください。多くのエディタでは「UTF-8（BOM なし）」または「UTF-8 without BOM」として保存できます。

### 1.3 McpServer.Con - エラーハンドリングの欠如

**問題点:**
- TimeZoneInfo.FindSystemTimeZoneById() で無効なタイムゾーンが指定された場合の処理が簡易的（行 34-40）
- try-catch ブロックが広すぎて、具体的なエラー情報が失われる
- エラーメッセージが日本語のみで、国際化を考慮していない

**修正案:**
より詳細なエラーハンドリングを実装してください。

```csharp
[McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
public static string GetTimeInTimezone(string timezone)
{
    try {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone).ToString();
    } catch (TimeZoneNotFoundException ex) {
        return $"タイムゾーン '{timezone}' が見つかりません: {ex.Message}";
    } catch (InvalidTimeZoneException ex) {
        return $"無効なタイムゾーン '{timezone}': {ex.Message}";
    } catch (Exception ex) {
        return $"エラーが発生しました: {ex.Message}";
    }
}
```

### 1.4 McpServer.Sse - ハードコードされたポート番号

**問題点:**
ポート番号 3001 がハードコードされています（行 28）。環境によってはポートが使用中の場合があります。

```csharp
application.Run("http://localhost:3001");
```

**修正案:**
設定ファイル（appsettings.json）または環境変数から読み込むようにしてください。

```csharp
// appsettings.json の読み込みは WebApplication.CreateBuilder(args) で自動的に行われる
// appsettings.json に以下を追加:
// {
//   "Urls": "http://localhost:3001"
// }

// または環境変数を使用
application.Run(); // ASPNETCORE_URLS 環境変数から読み込む
```

### 1.5 McpServer.Sse - CORS、認証、TLS の設定が不足

**問題点:**
コメント（行 11-12）で「ローカルでは TLS、認証、認可、CORS 設定などを検討する必要がある」と述べられていますが、実装されていません。

**修正案:**
開発環境でも基本的な CORS 設定を追加し、本番環境への移行を容易にしてください。

```csharp
var builder = WebApplication.CreateBuilder(args);

// CORS の設定例
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

var application = builder.Build();

// CORS を有効化
application.UseCors();

application.MapMcp();
application.Run("http://localhost:3001");
```

本番環境では、より厳格な CORS 設定、HTTPS、認証（JWT など）を追加してください。

### 1.6 McpServer.Sse - 天気予報のモックデータが不正確

**問題点:**
天気予報ツール（行 37-45）が固定データを返しており、実際の天気予報ではありません。また、デフォルト値が「晴か曇りか雨か雪か霙か霰か何か」という不明確な文字列になっています。

**修正案:**
サンプルコードであることを明確にし、より適切なエラーメッセージを返してください。

```csharp
[McpServerTool, Description("指定した場所の天気を予報します（サンプル用モックデータ）")]
public static string GetWeatherForecast(
    [Description("天気を予報したい都道府県名")]
    string location) => location switch {
        "北海道" => "晴れ（モックデータ）",
        "東京都" => "曇り（モックデータ）",
        "石川県" => "雨（モックデータ）",
        "福井県" => "雪（モックデータ）",
        _       => $"'{location}' の天気情報は利用できません（サンプル用のモックデータのため）"
    };
```

または、実際の天気 API を呼び出すように実装を変更してください。

---

## 2. 改善点

### 2.1 McpServer.Con - 日時フォーマットの改善

**問題点:**
DateTimeOffset.ToString() がカルチャに依存するデフォルトフォーマットを使用しています（行 27, 36）。

**改善案:**
明示的なフォーマットを指定してください。

```csharp
[McpServerTool, Description("現在の時刻を取得")]
public static string GetCurrentTime() 
    => DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");

[McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
public static string GetTimeInTimezone(string timezone)
{
    try {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        var timeInZone = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone);
        return timeInZone.ToString("yyyy-MM-dd HH:mm:ss zzz");
    } catch {
        return "無効なタイムゾーンが指定されています";
    }
}
```

### 2.2 McpServer.Con - タイムゾーンの例示が不明確

**問題点:**
コメント（行 30）でタイムゾーン ID の例として "Pacific Standard Time" と "Asia/Tokyo" が挙げられていますが、Windows と IANA の形式が混在しています。

**改善案:**
プラットフォームに応じた適切な例を示してください。

```csharp
// Windows: "Pacific Standard Time"、"Tokyo Standard Time"
// Unix/Linux: "America/Los_Angeles"、"Asia/Tokyo"
// .NET 6+ では TimeZoneConverter パッケージで統一可能
```

または、コメントを明確にしてください：

```csharp
// 引数 `timezone` はシステムのタイムゾーン ID。
// Windows: "Tokyo Standard Time", "Pacific Standard Time"
// Unix/Linux: "Asia/Tokyo", "America/Los_Angeles"
```

### 2.3 McpServer.Sse - ルーティングの明示化

**問題点:**
MCP のルーティングがどのエンドポイントにマップされるか不明確です（行 27）。

**改善案:**
コメントで明示的にエンドポイントを示してください。

```csharp
// MCP のルーティングをマップしてサーバーを起動
// デフォルトエンドポイント:
// - GET  /mcp/v1/capabilities : サーバーの機能一覧
// - POST /mcp/v1/tools/list   : 利用可能なツール一覧
// - POST /mcp/v1/tools/call   : ツールの実行
// - GET  /mcp/sse             : SSE エンドポイント（リアルタイム通信）
application.MapMcp();
application.Run("http://localhost:3001");
```

### 2.4 両プロジェクト - ロギングの追加

**問題点:**
エラー情報や動作状況をログに記録していないため、デバッグやトラブルシューティングが困難です。

**改善案:**
Microsoft.Extensions.Logging を使用したロギングを追加してください。

McpServer.Con の例：
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information))
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithToolsFromAssembly();

await builder.Build().RunAsync();
```

McpServer.Sse の例：
```csharp
var builder = WebApplication.CreateBuilder(args);

// デフォルトでロギングが有効
builder.Logging.AddConsole();

builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

var application = builder.Build();

application.Logger.LogInformation("MCP サーバーを起動します: http://localhost:3001");

application.MapMcp();
application.Run("http://localhost:3001");
```

### 2.5 両プロジェクト - ツールのパラメータ検証

**問題点:**
ツールメソッドで入力パラメータの検証が行われていません。

**改善案:**
パラメータの検証を追加してください。

McpServer.Con の例：
```csharp
[McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
public static string GetTimeInTimezone(string timezone)
{
    if (string.IsNullOrWhiteSpace(timezone))
    {
        return "エラー: タイムゾーンが指定されていません";
    }

    try {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone).ToString();
    } catch {
        return $"無効なタイムゾーンが指定されています: {timezone}";
    }
}
```

McpServer.Sse の例：
```csharp
[McpServerTool, Description("指定した場所の天気を予報します")]
public static string GetWeatherForecast(
    [Description("天気を予報したい都道府県名")]
    string location)
{
    if (string.IsNullOrWhiteSpace(location))
    {
        return "エラー: 場所が指定されていません";
    }

    return location switch {
        "北海道" => "晴れ",
        "東京都" => "曇り",
        "石川県" => "雨",
        "福井県" => "雪",
        _       => $"'{location}' の天気情報は利用できません"
    };
}
```

---

## 3. 可読性・わかりやすさ

### 3.1 McpServer.Con - 変数名の改善

**現状:**
変数名は適切ですが、より説明的にできます。

**改善案:**
```csharp
// Before
TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone).ToString();

// After
TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
DateTimeOffset currentTimeInZone = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, targetTimeZone);
return currentTimeInZone.ToString("yyyy-MM-dd HH:mm:ss zzz");
```

### 3.2 McpServer.Sse - コメントの改善

**現状:**
コメントは詳細ですが、構造化されていません。

**改善案:**
セクションを明確に分けてください。

```csharp
// ========================================
// Model Context Protocol (MCP) サーバー - HTTP/SSE トランスポート版
// ========================================
//
// 【概要】
// WebApplication を使って MCP サーバーをホストし、HTTP トランスポートを有効にすることで、
// SSE（Server-Sent Events）を使ったリアルタイム通信をサポートします。
//
// 【トランスポート】
// - stdio版（McpServer.Con）: 標準入出力経由で通信
// - HTTP/SSE版（このプロジェクト）: HTTP エンドポイント経由で通信
//
// 【起動方法】
// 1. このプログラムを実行すると http://localhost:3001 でサーバーが起動
// 2. クライアントは MCP のプロトコルに従って HTTP エンドポイント経由で接続
// 3. SSE でリアルタイム通信が可能
//
// 【ツールの定義】
// - ツールは [McpServerToolType] 属性を持つクラス内で定義
// - 各メソッドに [McpServerTool] 属性を付与して公開
// - Description 属性でツールの説明をクライアントに提供
//
// 【セキュリティ】
// - ローカル開発環境用のサンプルです
// - 本番環境では以下を検討してください:
//   - HTTPS (TLS) の有効化
//   - 認証・認可の実装
//   - CORS の適切な設定
//   - レート制限
//   - 入力検証
// ========================================
```

### 3.3 両プロジェクト - クラス名の一貫性

**現状:**
- McpServer.Con: `TimeTools`（複数形）
- McpServer.Sse: `WeatherForecastTool`（単数形）

**改善案:**
命名規則を統一してください。複数のツールを含む場合は複数形、単一のツールグループとして扱う場合は `Tools` サフィックスを使用することを推奨します。

```csharp
// McpServer.Sse
[McpServerToolType, Description("天気を予報する")]
public static class WeatherForecastTools  // 複数形に変更
{
    // ...
}
```

---

## 4. ドキュメント・説明の充実度

### 4.1 両プロジェクト - 前提条件の明記

**不足している情報:**
- 必要な .NET バージョン（net9.0）
- 依存パッケージのバージョン情報
- 実行環境の要件

**改善案:**
ファイル冒頭にセットアップ情報を追加してください。

```csharp
// ========================================
// 前提条件
// ========================================
// - .NET 9.0 SDK がインストールされていること
// - ModelContextProtocol パッケージ 0.4.0-preview.2 以降
// - （McpServer.Sse のみ）ポート 3001 が利用可能であること
//
// ========================================
// セットアップ手順
// ========================================
// 1. パッケージの復元
//    dotnet restore
//
// 2. プロジェクトのビルド
//    dotnet build
//
// 3. プログラムの実行
//    dotnet run --project McpServer.Con
//    または
//    dotnet run --project McpServer.Sse
//
// ========================================
// クライアントからの接続方法
// ========================================
// McpServer.Con (stdio):
// - 標準入出力を通じて MCP プロトコルで通信
// - クライアントはプロセスを起動し、stdin/stdout で通信
//
// McpServer.Sse (HTTP/SSE):
// - HTTP エンドポイント: http://localhost:3001
// - ツール一覧の取得: POST http://localhost:3001/mcp/v1/tools/list
// - ツールの実行: POST http://localhost:3001/mcp/v1/tools/call
// - SSE エンドポイント: GET http://localhost:3001/mcp/sse
// ========================================
```

### 4.2 McpServer.Sse - appsettings.json の説明不足

**問題点:**
appsettings.json ファイルが存在しますが、その内容や役割が説明されていません。

**改善案:**
設定ファイルの内容を確認し、コメントで説明してください。

### 4.3 両プロジェクト - ツールの使用例

**不足している情報:**
クライアントからツールを呼び出す具体的な例がありません。

**改善案:**
コメントに使用例を追加してください。

McpServer.Con の例：
```csharp
// ツール定義: 各メソッドは公開されるツールとして呼び出し可能になる
//
// 【使用例】
// ツール名: GetCurrentTime
// 引数: なし
// 戻り値: "2025-10-20 11:35:01 +09:00"
//
// ツール名: GetTimeInTimezone
// 引数: { "timezone": "Tokyo Standard Time" }  // Windows
//       { "timezone": "Asia/Tokyo" }            // Unix/Linux
// 戻り値: "2025-10-20 11:35:01 +09:00"
//
[McpServerToolType, Description("時刻を取得する")]
public static class TimeTools
{
    // ...
}
```

---

## 5. コード構造・順序

### 5.1 両プロジェクト - 適切な構造

**良い点:**
- トップレベルステートメントを使用した簡潔な記述
- ツール定義がファイル末尾に配置され、見通しが良い
- using ディレクティブが適切に配置されている

**オプションの改善案:**
大規模なプロジェクトへの拡張を見据えて、ツールを別ファイルに分離することを検討してください。

```
McpServer.Con/
├── Program.cs           // サーバーの起動処理のみ
├── Tools/
│   └── TimeTools.cs     // ツールの定義
└── McpServer.Con.csproj
```

### 5.2 McpServer.Sse - ミドルウェアの順序

**現状:**
ミドルウェアの設定がシンプルですが、将来的な拡張を考慮していません。

**改善案:**
ミドルウェアの推奨順序でコメントを追加してください。

```csharp
var application = builder.Build();

// ミドルウェアの設定（推奨順序）
// 1. 例外ハンドリング（開発環境）
if (application.Environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
}

// 2. HTTPS リダイレクト（本番環境）
// application.UseHttpsRedirection();

// 3. CORS
// application.UseCors();

// 4. 認証・認可
// application.UseAuthentication();
// application.UseAuthorization();

// 5. MCP ルーティング
application.MapMcp();

application.Run("http://localhost:3001");
```

---

## 6. 誤字・脱字・表現の問題

### 6.1 McpServer.Con - コメントの表記

**行 22:**
```csharp
[McpServerToolType, Description("時刻を取得する")] // このクラスがツールグループであることを示す属性
```

**指摘:**
「ツールグループ」という表現が曖昧です。

**修正案:**
```csharp
[McpServerToolType, Description("時刻を取得する")] 
// MCP ツールタイプとしてこのクラスを登録
// クラス内の [McpServerTool] 属性を持つメソッドがツールとして公開される
```

### 6.2 McpServer.Sse - コメントの誤記

**行 2-4（※ファイルは現在 Shift-JIS で保存されているため、UTF-8 に変換後のコード）:**
```csharp
// - WebApplication を使って MCP サーバーをホスト
// - HTTP トランスポートを有効にすることで、SSE（Server-Sent Events）を使ったリアルタイム通信をサポート
// - 同一アセンブリ内に定義された McpServer ツールを登録
```

**指摘:**
「McpServer ツール」という表現が曖昧です。正しくは「MCP ツール」または「ツール」です。

**修正案:**
```csharp
// - WebApplication を使って MCP サーバーをホスト
// - HTTP トランスポートを有効にすることで、SSE（Server-Sent Events）を使ったリアルタイム通信をサポート
// - 同一アセンブリ内に定義されたツールを登録
```

### 6.3 McpServer.Sse - デフォルトケースの表現

**行 44（※ファイルは現在 Shift-JIS で保存されているため、UTF-8 に変換後のコード）:**
```csharp
_       => "晴か曇りか雨か雪か霙か霰か何か"
```

**指摘:**
この表現は冗談として面白いですが、実用的ではありません。

**修正案:**
```csharp
_       => $"'{location}' の天気情報は利用できません"
// または
_       => "指定された場所の天気情報は登録されていません"
```

---

## 7. 表現の統一性

### 7.1 Description 属性の記述スタイル

**現状:**
- McpServer.Con: 「現在の時刻を取得」（名詞止め）
- McpServer.Sse: 「天気を予報します」（丁寧語）

**改善案:**
統一したスタイルを使用してください。推奨は名詞止めまたは「〜する」です。

```csharp
// McpServer.Con
[McpServerTool, Description("現在の時刻を取得")]
[McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]

// McpServer.Sse
[McpServerTool, Description("指定した場所の天気を予報")]
// または
[McpServerTool, Description("指定した場所の天気予報を取得")]
```

### 7.2 コメントのスタイル統一

**現状:**
両プロジェクトでコメントのスタイルが異なります。

**改善案:**
共通のテンプレートを使用してください。

```csharp
// ========================================
// [プロジェクト名] - [簡易説明]
// ========================================
//
// 【概要】
// [詳細な説明]
//
// 【アーキテクチャ】
// [使用技術やパターンの説明]
//
// 【起動方法】
// [実行手順]
//
// 【注意事項】
// [セキュリティや制限事項]
// ========================================
```

---

## 8. 内容の正確性

### 8.1 技術的な正確性

**検証項目:**
- ✅ ModelContextProtocol パッケージの使用方法は正しい
- ✅ 属性ベースのツール定義方法は正しい
- ✅ トランスポートの設定方法は正しい
- ⚠️ McpServer.Sse のエンコーディングエラーでビルドが失敗する（修正必須）
- ⚠️ McpServer.Con に BOM が含まれる（修正推奨）

### 8.2 タイムゾーン ID の互換性

**問題点:**
コメント（McpServer.Con 行 30）で "Asia/Tokyo" を例示していますが、これは IANA タイムゾーンデータベースの形式であり、Windows では動作しません。

**修正案:**
プラットフォーム依存性を明記してください。

```csharp
// 引数 `timezone` はシステムのタイムゾーン ID
// ※ プラットフォームによって形式が異なります:
//   - Windows: "Tokyo Standard Time", "Pacific Standard Time"
//   - Unix/Linux/macOS: "Asia/Tokyo", "America/Los_Angeles"
// ※ クロスプラットフォーム対応には TimeZoneConverter パッケージの使用を推奨
```

---

## 9. 重複・冗長性

### 9.1 不要な重複の確認

**現状:**
両プロジェクトとも、不要な重複はありません。コードは簡潔で、適切に構造化されています。

### 9.2 共通コードの抽出（オプション）

**改善案（将来的な拡張を考慮）:**
両プロジェクトで共通する設定やヘルパーメソッドがある場合、共有ライブラリに抽出することを検討してください。

```
Shos.AIAgentSample/
├── McpServer.Con/
├── McpServer.Sse/
├── McpServer.Common/        # 共通ライブラリ（新規作成）
│   ├── Tools/               # 共通ツール
│   ├── Validators/          # 入力検証
│   └── Extensions/          # 拡張メソッド
```

---

## 10. その他の問題点

### 10.1 セキュリティ

**問題点:**

1. **入力検証の欠如:**
   - ツールのパラメータに対する検証がない
   - インジェクション攻撃のリスク（現時点では低いが、拡張時に注意）

2. **エラー情報の漏洩:**
   - 例外メッセージをそのまま返すと、内部情報が漏洩する可能性

3. **HTTP 通信（McpServer.Sse）:**
   - 平文通信のため、機密情報の送受信には不適切

**改善案:**
```csharp
// 入力検証の例
public static string GetTimeInTimezone(string timezone)
{
    if (string.IsNullOrWhiteSpace(timezone))
    {
        return "エラー: タイムゾーンが指定されていません";
    }

    // タイムゾーン ID の形式を検証
    if (timezone.Length > 100) // 異常に長い入力を拒否
    {
        return "エラー: 無効な入力です";
    }

    try {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone)
                          .ToString("yyyy-MM-dd HH:mm:ss zzz");
    } catch (TimeZoneNotFoundException) {
        // 詳細なエラーメッセージを返さず、一般的なメッセージを返す
        return "エラー: 指定されたタイムゾーンが見つかりません";
    } catch {
        // 内部エラーの詳細を隠す
        return "エラー: 処理中に問題が発生しました";
    }
}
```

### 10.2 パフォーマンス

**問題点:**
特に問題はありませんが、以下の最適化を検討できます。

1. **キャッシング:**
   - TimeZoneInfo.FindSystemTimeZoneById() の結果をキャッシュ

2. **非同期処理:**
   - 現在のツールは同期メソッドですが、I/O バウンドの処理（API 呼び出しなど）を追加する場合は非同期化を検討

**改善案:**
```csharp
// タイムゾーンのキャッシュ例
private static readonly Dictionary<string, TimeZoneInfo> _timeZoneCache = new();

[McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
public static string GetTimeInTimezone(string timezone)
{
    try {
        if (!_timeZoneCache.TryGetValue(timezone, out var timeZone))
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            _timeZoneCache[timezone] = timeZone;
        }
        
        return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone)
                          .ToString("yyyy-MM-dd HH:mm:ss zzz");
    } catch {
        return "無効なタイムゾーンが指定されています";
    }
}
```

### 10.3 テスタビリティ

**問題点:**
トップレベルステートメントで記述されているため、ユニットテストが困難です。

**改善案:**
テスト可能な設計にするため、ツールクラスを別ファイルに分離し、依存性注入を活用してください。

McpServer.Con の例：
```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithToolsFromAssembly();

await builder.Build().RunAsync();

// Tools/TimeTools.cs (別ファイル)
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer.Con.Tools;

[McpServerToolType, Description("時刻を取得する")]
public static class TimeTools
{
    [McpServerTool, Description("現在の時刻を取得")]
    public static string GetCurrentTime() 
        => DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");

    [McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
    public static string GetTimeInTimezone(string timezone)
    {
        // 実装
    }
}

// Tests/TimeToolsTests.cs (テストプロジェクト)
using Xunit;
using McpServer.Con.Tools;

public class TimeToolsTests
{
    [Fact]
    public void GetCurrentTime_ReturnsFormattedTime()
    {
        // Act
        string result = TimeTools.GetCurrentTime();

        // Assert
        Assert.NotNull(result);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} [+-]\d{2}:\d{2}", result);
    }

    [Theory]
    [InlineData("Tokyo Standard Time")]  // Windows
    [InlineData("Asia/Tokyo")]            // Unix
    public void GetTimeInTimezone_ValidTimezone_ReturnsTime(string timezone)
    {
        // Act
        string result = TimeTools.GetTimeInTimezone(timezone);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("無効", result);
    }

    [Fact]
    public void GetTimeInTimezone_InvalidTimezone_ReturnsErrorMessage()
    {
        // Act
        string result = TimeTools.GetTimeInTimezone("Invalid/Timezone");

        // Assert
        Assert.Contains("無効", result);
    }
}
```

### 10.4 国際化（i18n）

**問題点:**
エラーメッセージやツールの説明が日本語のみです。

**改善案:**
リソースファイルを使用して多言語対応を検討してください。

```csharp
// Resources/Messages.ja.resx
// - CurrentTimeDescription: "現在の時刻を取得"
// - InvalidTimezoneError: "無効なタイムゾーンが指定されています"

// Resources/Messages.en.resx
// - CurrentTimeDescription: "Get current time"
// - InvalidTimezoneError: "Invalid timezone specified"

[McpServerTool, Description(Resources.Messages.CurrentTimeDescription)]
public static string GetCurrentTime() 
    => DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
```

---

## 総評

### 良い点

- ✅ Model Context Protocol の基本的な実装として優れたサンプル
- ✅ 2 つの異なるトランスポート（stdio と HTTP/SSE）を示している
- ✅ 属性ベースの宣言的なツール定義が簡潔で理解しやすい
- ✅ トップレベルステートメントを活用した最新の C# スタイル
- ✅ コメントが日本語で丁寧に記述されている
- ✅ 依存関係が最小限で、学習用サンプルとして適切

### 重要な改善点

#### 必須（ビルドエラー）
1. **McpServer.Sse のエンコーディング修正**（最優先）
   - Shift-JIS から UTF-8（BOM なし）に変換

#### 必須（品質向上）
2. **エラーハンドリングの強化**
   - 適切な例外処理の追加
   - ユーザーフレンドリーなエラーメッセージ

3. **入力検証の追加**
   - すべてのツールパラメータの検証
   - セキュリティリスクの軽減

4. **BOM の除去**（McpServer.Con）
   - UTF-8 BOM なしで保存

#### 推奨（保守性向上）
5. **設定の外部化**
   - ハードコードされた値を設定ファイルまたは環境変数に移動
   - ポート番号、タイムゾーン、その他の設定値

6. **ロギングの追加**
   - Microsoft.Extensions.Logging を使用
   - デバッグとトラブルシューティングの改善

7. **ドキュメントの充実**
   - 前提条件、セットアップ手順、使用例の追加
   - README.md の作成

8. **セキュリティの強化**（McpServer.Sse）
   - CORS の設定
   - HTTPS の有効化（本番環境）
   - 認証・認可の実装（本番環境）

#### オプション（拡張性向上）
9. **テスト可能な設計への変更**
   - ツールクラスの分離
   - ユニットテストの追加

10. **コード構造の改善**
    - 大規模プロジェクトへの拡張を見据えた構造化
    - 共通ライブラリの抽出

### 優先度マトリックス

| 優先度 | 項目 | 対象 | 理由 |
|--------|------|------|------|
| 最高 | エンコーディング修正 | McpServer.Sse | ビルドエラーの解消 |
| 高 | BOM 除去 | McpServer.Con | 潜在的な問題の防止 |
| 高 | エラーハンドリング | 両方 | 堅牢性の向上 |
| 高 | 入力検証 | 両方 | セキュリティとエラー防止 |
| 中 | 設定の外部化 | 両方 | 保守性の向上 |
| 中 | ロギング | 両方 | デバッグ容易性の向上 |
| 中 | ドキュメント充実 | 両方 | 利用者の理解促進 |
| 中 | CORS 設定 | McpServer.Sse | セキュリティとクロスオリジン対応 |
| 低 | テスト可能な設計 | 両方 | 品質保証（将来的な改善） |
| 低 | 国際化対応 | 両方 | グローバル展開（将来的な改善） |

---

## 推奨される次のステップ

### ステップ 1: 緊急対応（必須）
1. McpServer.Sse/Program.cs を UTF-8（BOM なし）に変換
2. McpServer.Con/Program.cs の BOM を除去
3. ビルドエラーの解消を確認

### ステップ 2: エラーハンドリングと検証（必須）
1. すべてのツールメソッドに try-catch ブロックを追加
2. パラメータの null/空文字チェックを追加
3. 適切なエラーメッセージを返すように修正

### ステップ 3: 設定とドキュメント（推奨）
1. appsettings.json を作成し、設定値を外部化
2. README.md を作成し、以下を記載：
   - 前提条件
   - セットアップ手順
   - 実行方法
   - ツールの使用例
   - トラブルシューティング
3. コメントを充実させる

### ステップ 4: セキュリティとロギング（推奨）
1. Microsoft.Extensions.Logging を追加
2. CORS 設定を追加（McpServer.Sse）
3. HTTPS 対応の準備（本番環境用）
4. 入力値のサニタイズ

### ステップ 5: テストと拡張性（オプション）
1. ツールクラスを別ファイルに分離
2. ユニットテストプロジェクトを作成
3. 基本的なテストケースを実装
4. CI/CD パイプラインの検討

---

## 具体的な修正例（完全版）

### McpServer.Con/Program.cs（修正版）

```csharp
// ========================================
// Model Context Protocol (MCP) サーバー - stdio トランスポート版
// ========================================
//
// 【概要】
// 標準入出力（stdio）経由で MCP サーバーを提供する最もシンプルな実装です。
// クライアントはプロセスを起動し、stdin/stdout を通じて MCP プロトコルで通信します。
//
// 【前提条件】
// - .NET 9.0 SDK
// - ModelContextProtocol パッケージ 0.4.0-preview.2 以降
//
// 【実行方法】
// dotnet run --project McpServer.Con
//
// 【トランスポート】
// - stdio: 標準入出力経由で通信
// - プロセス起動型のクライアントに適している
//
// 【比較】
// - McpServer.Con（このプロジェクト）: stdio トランスポート
// - McpServer.Sse: HTTP/SSE トランスポート
// ========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

// アプリケーションビルダーを作成し、MCP サーバーを登録して実行
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddLogging(logging => 
       {
           logging.AddConsole();
           logging.SetMinimumLevel(LogLevel.Information);
       })
       .AddMcpServer()               // MCP サーバーの基本サービスを登録
       .WithStdioServerTransport()   // 標準入出力経由のトランスポートを有効化
       .WithToolsFromAssembly();     // このアセンブリ内のツールを自動登録

await builder.Build().RunAsync();

// ========================================
// ツール定義: 時刻関連の操作
// ========================================
//
// 【使用例】
// ツール名: GetCurrentTime
// 引数: なし
// 戻り値: "2025-10-20 11:35:01 +09:00"
//
// ツール名: GetTimeInTimezone
// 引数: { "timezone": "Tokyo Standard Time" }  // Windows
//       { "timezone": "Asia/Tokyo" }            // Unix/Linux
// 戻り値: "2025-10-20 11:35:01 +09:00"
//
[McpServerToolType, Description("時刻を取得する")]
public static class TimeTools
{
    // タイムゾーンのキャッシュ（パフォーマンス最適化）
    private static readonly Dictionary<string, TimeZoneInfo> _timeZoneCache = new();
    private static readonly object _cacheLock = new();

    /// <summary>
    /// 現在の時刻を取得します。
    /// </summary>
    /// <returns>ISO 8601 形式の現在時刻</returns>
    [McpServerTool, Description("現在の時刻を取得")]
    public static string GetCurrentTime() 
        => DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");

    /// <summary>
    /// 指定されたタイムゾーンの現在の時刻を取得します。
    /// </summary>
    /// <param name="timezone">タイムゾーン ID
    /// Windows: "Tokyo Standard Time", "Pacific Standard Time"
    /// Unix/Linux/macOS: "Asia/Tokyo", "America/Los_Angeles"
    /// </param>
    /// <returns>指定されたタイムゾーンの現在時刻、またはエラーメッセージ</returns>
    [McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
    public static string GetTimeInTimezone(string timezone)
    {
        // 入力検証
        if (string.IsNullOrWhiteSpace(timezone))
        {
            return "エラー: タイムゾーンが指定されていません";
        }

        if (timezone.Length > 100)
        {
            return "エラー: タイムゾーン ID が長すぎます";
        }

        try 
        {
            // キャッシュから取得または新規作成
            TimeZoneInfo timeZone;
            lock (_cacheLock)
            {
                if (!_timeZoneCache.TryGetValue(timezone, out timeZone))
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                    _timeZoneCache[timezone] = timeZone;
                }
            }
            
            DateTimeOffset currentTimeInZone = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone);
            return currentTimeInZone.ToString("yyyy-MM-dd HH:mm:ss zzz");
        } 
        catch (TimeZoneNotFoundException) 
        {
            return $"エラー: タイムゾーン '{timezone}' が見つかりません。" +
                   "Windows では 'Tokyo Standard Time'、Unix/Linux では 'Asia/Tokyo' などの形式を使用してください。";
        } 
        catch (InvalidTimeZoneException) 
        {
            return $"エラー: '{timezone}' は無効なタイムゾーン ID です";
        }
        catch (Exception ex)
        {
            // 予期しないエラー（内部エラーの詳細を隠す）
            Console.Error.WriteLine($"Internal error: {ex}");
            return "エラー: 処理中に問題が発生しました";
        }
    }
}
```

### McpServer.Sse/Program.cs（修正版）

```csharp
// ========================================
// Model Context Protocol (MCP) サーバー - HTTP/SSE トランスポート版
// ========================================
//
// 【概要】
// WebApplication を使って MCP サーバーをホストし、HTTP トランスポートを有効にすることで、
// SSE（Server-Sent Events）を使ったリアルタイム通信をサポートします。
//
// 【前提条件】
// - .NET 9.0 SDK
// - ModelContextProtocol.AspNetCore パッケージ 0.4.0-preview.2 以降
// - ポート 3001 が利用可能であること（設定変更可能）
//
// 【実行方法】
// dotnet run --project McpServer.Sse
//
// 【エンドポイント】
// - サーバー: http://localhost:3001
// - 機能一覧: GET  http://localhost:3001/mcp/v1/capabilities
// - ツール一覧: POST http://localhost:3001/mcp/v1/tools/list
// - ツール実行: POST http://localhost:3001/mcp/v1/tools/call
// - SSE接続: GET  http://localhost:3001/mcp/sse
//
// 【トランスポート】
// - HTTP/SSE: HTTP エンドポイント経由で通信し、SSE でリアルタイム通信
// - Web アプリケーションやブラウザベースのクライアントに適している
//
// 【比較】
// - McpServer.Con: stdio トランスポート
// - McpServer.Sse（このプロジェクト）: HTTP/SSE トランスポート
//
// 【セキュリティ】
// このサンプルはローカル開発環境用です。本番環境では以下を検討してください:
// - HTTPS (TLS) の有効化
// - 認証・認可の実装（JWT、OAuth2 など）
// - CORS の適切な設定
// - レート制限
// - 入力検証とサニタイズ
// - ログ監視とアラート
// ========================================

using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// ロギングの設定
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// CORS の設定（開発環境用）
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // 開発環境では任意のオリジンを許可
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // 本番環境では特定のオリジンのみを許可
            // policy.WithOrigins("https://yourdomain.com")
            //       .AllowAnyMethod()
            //       .AllowAnyHeader();
        }
    });
});

// MCP サーバーをサービスに追加し、HTTP トランスポート（SSE 対応）とアセンブリ内のツールを登録
builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

var application = builder.Build();

// ミドルウェアの設定（推奨順序）
if (application.Environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
}
else
{
    // 本番環境では HTTPS にリダイレクト
    // application.UseHttpsRedirection();
}

// CORS を有効化
application.UseCors();

// 認証・認可（本番環境では有効化）
// application.UseAuthentication();
// application.UseAuthorization();

// MCP のルーティングをマップ
application.MapMcp();

// サーバーの起動
var url = builder.Configuration["Urls"] ?? "http://localhost:3001";
application.Logger.LogInformation("MCP サーバーを起動します: {Url}", url);
application.Logger.LogInformation("利用可能なエンドポイント:");
application.Logger.LogInformation("  - 機能一覧: GET  {Url}/mcp/v1/capabilities", url);
application.Logger.LogInformation("  - ツール一覧: POST {Url}/mcp/v1/tools/list", url);
application.Logger.LogInformation("  - ツール実行: POST {Url}/mcp/v1/tools/call", url);
application.Logger.LogInformation("  - SSE接続: GET  {Url}/mcp/sse", url);

application.Run(url);

// ========================================
// ツール定義: 天気予報（サンプル用モックデータ）
// ========================================
//
// 【注意】
// これはサンプル用のモックデータです。実際の天気予報を取得するには、
// 気象 API（OpenWeatherMap、Weather API など）を統合してください。
//
// 【使用例】
// ツール名: GetWeatherForecast
// 引数: { "location": "東京都" }
// 戻り値: "曇り（サンプル用モックデータ）"
//
[McpServerToolType, Description("天気を予報する")]
public static class WeatherForecastTools
{
    /// <summary>
    /// 指定した場所の天気を予報します（サンプル用モックデータ）。
    /// </summary>
    /// <param name="location">天気を予報したい都道府県名</param>
    /// <returns>天気予報の結果、またはエラーメッセージ</returns>
    [McpServerTool, Description("指定した場所の天気を予報（サンプル用モックデータ）")]
    public static string GetWeatherForecast(
        [Description("天気を予報したい都道府県名")]
        string location)
    {
        // 入力検証
        if (string.IsNullOrWhiteSpace(location))
        {
            return "エラー: 場所が指定されていません";
        }

        if (location.Length > 100)
        {
            return "エラー: 場所名が長すぎます";
        }

        // サンプル用のモックデータを返す
        // 実際の実装では、天気 API を呼び出してください
        return location switch 
        {
            "北海道" => "晴れ（サンプル用モックデータ）",
            "東京都" => "曇り（サンプル用モックデータ）",
            "石川県" => "雨（サンプル用モックデータ）",
            "福井県" => "雪（サンプル用モックデータ）",
            _       => $"'{location}' の天気情報は利用できません（サンプル用のモックデータのため、登録されていない場所です）"
        };
    }
}
```

### appsettings.json の追加（McpServer.Sse）

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Urls": "http://localhost:3001",
  "AllowedHosts": "*"
}
```

### appsettings.Development.json の追加（McpServer.Sse）

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

---

このコードレビューは、両プロジェクトの品質向上とベストプラクティスの適用を目的としています。学習用サンプルとしては既に優れていますが、上記の改善を実施することで、より実用的で堅牢なサンプルコードになります。
