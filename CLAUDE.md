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

| ステップ | コンソール | Web | 内容 | チュートリアル |
|---|---|---|---|---|
| 1 | FCAIAgent1 | FCAIChat1 | Ollama で単発プロンプト | 1.1 |
| 2 | FCAIAgent2 | FCAIChat2 | Ollama / Azure OpenAI 切替 (`ChatClientType`) | 1.2 |
| 3 | FCAIAgent3 | FCAIChat3 | `AgentSession` による複数ターン | 1.3 |
| 4 | FCAIAgent4 | FCAIChat4 | STDIO MCP サーバー (McpServer.Con) のツール利用 | 3.1 |
| 5 | FCAIAgent5 | FCAIChat5 | 複数 MCP サーバー (time / weather / filesystem) | 3.2 |
| 最終 | FCAIAgent | — | FCAIAgent5 と同等 | — |

`Documents/tutorial.*.md` には各ステップのコードがそのまま掲載されているため、コードを変更したら対応するチュートリアルのコード片も合わせて更新する必要がある。

### コンソールエージェント (FCAIAgent*)

トップレベルステートメントの `Program.cs` 1 ファイル構成。自作の型・定数・メソッドはすべて `static class My` にまとめる規約（ライブラリの API と見分けやすくするため）。流れ：`My.GetChatClient()` で `IChatClient` を生成（`.AsBuilder().UseFunctionInvocation()` でツール呼び出し有効化）→ MCP クライアントからツール取得 → `ChatClientAgent` 作成 → `AgentSession` で対話ループ → 終了時に `McpClient.DisposeAsync()`。

- Ollama: `http://localhost:11434`、モデルは `ollama.SelectedModel` で指定
- Azure OpenAI: 環境変数 `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_API_KEY`、デプロイ名は `deploymentName` 定数
- MCP の STDIO サーバーパスは `C:\Source\FCAIAgentSample\...` のようにハードコードされており、環境に応じて書き換える前提

### MCP サーバー

- `McpServer.Con`: STDIO トランスポート。時刻取得ツール
- `McpServer.Sse`: ASP.NET Core + `ModelContextProtocol.AspNetCore`、`http://localhost:3001` 固定。天気予報ツール
- どちらも `[McpServerToolType]` クラスの `[McpServerTool, Description(...)]` メソッドを `.WithToolsFromAssembly()` で自動登録

### Web チャット (FCAIChat*)

ASP.NET Core Razor Pages + SignalR + Identity + EF Core (SQL Server LocalDB)。

- `Hubs/ChatHub.cs`: メッセージを全クライアントへブロードキャストし DB (`MessagesDbContext`) に保存。メッセージに `@<エージェント名>` が含まれる場合のみエージェントに転送する
- `AIAgents/ChatAgent.cs`: 抽象 `ChatAgent`（クライアント生成・MCP 接続・セッションのシリアライズ/デシリアライズ）と具象 `MyChatAgent`。SignalR の接続 ID ごとにインスタンスを保持
- `Services/IThreadStore` / `InMemoryThreadStore`: シリアライズしたセッション (`JsonElement`) を接続 ID 単位で保存し再接続時に復元

### FCAICad1

Windows Forms の CAD アプリ。`ChatAgent.cs` が `GetPaperSize` / `DrawLine` などを AI 関数ツールとして公開し、自然言語の指示で図形を描画する。Windows 専用。

## その他

- `Reviews/` は過去のコード/ドキュメントレビュー結果
- `.mcp.json` / `.vscode/mcp.json` は MCP サーバー設定のサンプル
- Microsoft Agent Framework / MCP SDK は API 変更が多い。パッケージ更新時は全プロジェクトとチュートリアルのコードを揃えてビルド確認すること
