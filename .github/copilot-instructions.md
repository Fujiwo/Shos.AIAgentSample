# copilot-instructions.md

このリポジトリは、AI エージェント開発ハンズオンセミナー用のサンプルコードと日本語チュートリアルです (C# / .NET 10、Microsoft Agent Framework、Model Context Protocol)。

## 構成上の約束事

- プロジェクトは学習ステップごとの完全なコピーです (FCAIAgent1〜5、FCAIChat1〜5)。共通ライブラリはありません。
- モデル名・デプロイ名・パッケージのバージョン・チャットクライアント生成など、共通部分を変更する場合は**全プロジェクトに同じ変更を適用**してください。
- `Documents/tutorial.*.md` には各ステップのコードがそのまま掲載されています。コードを変更したら、対応するチュートリアルのコード片も更新してください。
- コンソール版では、自作の型・定数・メソッドを `static class My` にまとめます (ライブラリの API と区別するため)。
- ステップ間の差分は「// 新: ここから」「// 旧: ここまで」などのコメントで示します。
- コメントとドキュメントは日本語で書きます。
- C# 14 (.NET 10) の書き方を使います: ファイルスコープの名前空間、プライマリコンストラクター、コレクション式、拡張ブロック (`extension`)、`field` キーワード。

## 設定値

- Ollama: `http://localhost:11434`、モデル `minimax-m3:cloud`
- Azure OpenAI: 環境変数 `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_API_KEY`、デプロイ名 `MAI-Thinking-1`
- MCP (HTTP) サーバー: `http://localhost:3001/` (Streamable HTTP。`/sse` は既定で無効)
