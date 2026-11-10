# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 概要

AI エージェント開発ハンズオンセミナー用のサンプルコードと日本語チュートリアル。Microsoft Agent Framework (`Microsoft.Agents.AI`) + Model Context Protocol (`ModelContextProtocol`) を使った C# / .NET 10 のサンプル群。コード・コメント・ドキュメントはすべて日本語で書かれている。

## ビルド・実行

```bash
dotnet build Shos.AIAgentSample.sln          # 全体ビルド (FCAICad1 は net10.0-windows のため Windows 以外ではビルド不可)
dotnet build FCAIAgent3                      # 単一プロジェクト
dotnet run --project FCAIAgent5              # コンソールエージェント
dotnet run --project McpServer.Sse           # http://localhost:3001 で MCP (HTTP/SSE) サーバー起動
```

テストプロジェクト・Lint 設定は存在しない。動作確認は実行して行う（実行には Ollama または Azure OpenAI が必要）。

## アーキテクチャ

### 段階的学習構成（番号＝チュートリアルのステップ）

プロジェクトは「前のステップを拡張した完全なコピー」として並んでおり、共通ライブラリは無い。同じコードが複数プロジェクトに重複しているため、**共通部分（モデル名・パッケージバージョン・チャットクライアント生成など）を変更する場合は全プロジェクトに同じ変更を適用する**のがこのリポジトリの慣例（直近のコミットも一括更新）。

| ステップ | コンソール | 内容 | チュートリアル |
|---|---|---|---|
| 1 | FCAIAgent1 | Ollama で単発プロンプト | 1.1 |
| 2 | FCAIAgent2 | Ollama / Azure OpenAI 切替 (`ChatClientType`) | 1.2 |
| 3 | FCAIAgent3 | `AgentSession` による複数ターン | 1.3 |
| 4 | FCAIAgent4 | STDIO MCP サーバー (McpServer.Con) のツール利用 | 3.1 |
| 5 | FCAIAgent5 | 複数 MCP サーバー (time / weather / filesystem) | 3.2 |
| 最終 | FCAIAgent | FCAIAgent5 から「旧/新」コメントを除いた完成形 | — |

Web 版 (FCAIChat1〜5) はチュートリアル文書とは独立した系列 (ただし段階的学習用で、下記の必須条件が適用される): 1 = SignalR チャット、2 = EF Core で DB 保存、3 = 部分ビュー表示 (1〜3 は LLM を使わない)、4 = AI エージェント + Playwright MCP、5 = Brave Search MCP + ユーザー単位の会話保存・復元。

#### 段階的学習用コードの必須条件

- **FCAIAgent1〜5 はチュートリアルであり、受講者に段階的に (ステップバイステップで) プログラミングしてもらうためのもの。前のステップとの差分が分かるように書かなければならない**
- **FCAIAgent は、FCAIAgent5 から差分に関するコメントを消したものでなければならない** (コードは FCAIAgent5 と完全に一致させる。違ってよいのは差分コメント (`旧/新` の印と旧コード) の有無と、冒頭コメントの実行方法のプロジェクト名だけ)。このため FCAIAgent5 では、印の行に FCAIAgent でも残すべき説明を書かず、`// 新: ここから` の次の行に通常のコメントとして書く
- **FCAIChat1〜5 もチュートリアルであり、段階的にプログラミングしてもらうためのもの。前のステップとの差分が分かるように書かなければならない**

差分の示し方 (FCAIAgent2〜5 の書き方に合わせる):

- 追加したコードは `// 新: ここから` 〜 `// 新: ここまで` (1 行なら `// 新: ...`) で囲む
- 削除・置換したコードは消さずにコメントアウトして `// 旧: ここから` 〜 `// 旧: ここまで` (1 行なら `// 旧:`) で囲み、その直後に新しいコードを `新:` で示す。旧コードは前ステップのコードと一字一句同じにする
- 印は「直前のステップからの差分」だけに付ける。前のステップの `旧/新` コメントは次のステップでは外し、旧コードは削除する
- 既存行の引数追加・アクセス修飾子変更・`try` での囲み込みなど、既存コードの変更にも印を付ける。印の無い変更を作らない
- 差分を増やすだけの変更 (整形・桁揃え・改行コード・BOM・無関係なリファクタリング・行の並べ替え) をステップ間で入れない。共通部分を直すときは全ステップに同じ変更を入れる
- Razor (`.cshtml`) は `@* 新: ここから *@`、CSS は `/* 新: ここから */`、JavaScript は `// 新: ここから`、csproj は `<!-- 新: ここから -->` のように各言語のコメントで同じ印を付ける。Razor の旧コードは `@* 旧: ここから` 〜 `旧: ここまで *@` の 1 つのコメントで囲む
- そのステップで追加したファイルは、先頭 (Razor は `@page` / `@model` の直後) に `// 新: このステップで追加したファイル (説明)` を書く
- コメントだけの変更には印は不要。自動生成されたファイル (`Migrations/`、`wwwroot/lib/`) と画像も対象外

`Documents/tutorial.*.md` には FCAIAgent1〜5 / McpServer.Con / McpServer.Sse の `Program.cs` 全体がそのまま掲載されている (各ファイルで一番長い ```csharp ブロック)。コードを変更したら、対応するチュートリアルのコード片も同じ内容に更新する。

### コンソールエージェント (FCAIAgent*)

トップレベルステートメントの `Program.cs` 1 ファイル構成。自作の型・定数・メソッドはすべて `static class My` にまとめる規約（ライブラリの API と見分けやすくするため）。流れ：`My.GetChatClient()` で `IChatClient` を生成（`.AsBuilder().UseFunctionInvocation()` でツール呼び出し有効化）→ MCP クライアントからツール取得 → `ChatClientAgent` 作成 → `AgentSession` で対話ループ → 終了時に `McpClient.DisposeAsync()`。

- Ollama: `http://localhost:11434`、モデルは `ollama.SelectedModel` で指定
- Azure OpenAI: 環境変数 `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_API_KEY`、デプロイ名は `deploymentName` 定数
- MCP の STDIO サーバーパスは `C:\Source\FCAIAgentSample\...` のようにハードコードされており、環境に応じて書き換える前提

### MCP サーバー

- `McpServer.Con`: STDIO トランスポート。時刻取得ツール
- `McpServer.Sse`: ASP.NET Core + `ModelContextProtocol.AspNetCore`、`http://localhost:3001/` 固定。天気予報ツール。名前に反して Streamable HTTP で、旧来の `/sse` エンドポイントは既定で無効 (`EnableLegacySse` の既定値が false)
- どちらも `[McpServerToolType]` クラスの `[McpServerTool, Description(...)]` メソッドを `.WithToolsFromAssembly()` で自動登録

### Web チャット (FCAIChat*)

ASP.NET Core Razor Pages + SignalR + Identity + EF Core (SQL Server LocalDB)。

- `Hubs/ChatHub.cs`: メッセージを全クライアントへブロードキャストし DB (`MessagesDbContext`) に保存。メッセージに `@<エージェント名>` が含まれる場合のみエージェントに転送する。Hub は Transient なので、状態 (エージェント) を Hub のフィールドに持たないこと
- `AIAgents/ChatAgent.cs`: 抽象 `ChatAgent`（クライアント生成・MCP 接続・セッション管理、`IAsyncDisposable`）と具象 `MyChatAgent`
- `AIAgents/ChatAgentManager.cs`: Singleton。ユーザー (`Context.UserIdentifier`) ごとにエージェントを保持し、そのユーザーの最後の接続が切れたら破棄 (MCP の子プロセスも終了)
- FCAIChat5 のみ: `Services/IThreadStore` / `InMemoryThreadStore` にユーザー単位でセッション (`JsonElement`) を保存し、再接続時に `RestoreSession` で復元。Hub は `[Authorize]`

### FCAICad1

Windows Forms の CAD アプリ。`Toolbox.cs` の静的メソッド (`GetPaperSize` / `DrawLine` など) を `AIFunctionFactory.Create` で AI 関数ツールとして公開し、自然言語の指示で図形を描画する。ツールは UI スレッド以外から呼ばれうるため、フォーム操作は `MainForm.InvokeOnUIThread` 経由で行う。Brave Search MCP は任意 (なくても動作)。Windows 専用だが、Linux でも `-p:EnableWindowsTargeting=true` でビルド (コンパイル確認) はできる。

## コーディングスタイル (C# 14 / .NET 10)

- ファイルスコープの名前空間、プライマリコンストラクター、コレクション式 (`[]`)、ターゲット型の `new()` を使う
- 拡張メンバーは C# 14 の拡張ブロック (`extension(T receiver) { ... }`) で書く (例: `FCAICad1/Helpers.cs`、`Model.cs` の `GeometryExtensions`)
- バッキングフィールドが必要なプロパティは C# 14 の `field` キーワードを使う (例: `PointsFigure.Points`)
- 例外として、コンソール版の `AIAgent agent = new ChatClientAgent(...)` のように、教材として型名を見せている箇所は明示的な型のままにする
- JavaScript (`FCAIChat*/wwwroot/js/chat.js`) は ES2022 の ES モジュールとして書き、`<script type="module">` で読み込む。`DOMContentLoaded` を待たず、SignalR の接続はトップレベル await で開始する。`const`/`let`、アロー関数、`async`/`await` を使う (`wwwroot/lib/` のサードパーティは対象外)
- CSS (`wwwroot/css/site.css`) は UTF-8 で保存し、ネイティブの CSS ネスト、`::before`/`::after`、メディアクエリの範囲構文 (`width >= 768px`)、論理プロパティ (`inset-*`, `margin-inline-*`, `float: inline-end`)、スペース区切りの色 (`rgb(0 0 0 / 5%)`)、カスタムプロパティを使う。スコープ付き CSS (`*.cshtml.css`) は ASP.NET の CSS 分離でセレクターが書き換えられるため、ネストや `:is()` は使わない

## その他

- `Reviews/` は過去のコード/ドキュメントレビュー結果
- `.mcp.json` / `.vscode/mcp.json` は MCP サーバー設定のサンプル
- Microsoft Agent Framework / MCP SDK は API 変更が多い。パッケージ更新時は全プロジェクトとチュートリアルのコードを揃えてビルド確認すること
