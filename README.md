# Shos.AIAgentSample

AIエージェント開発ハンズオンセミナー (開発者向け) のサンプルコードとチュートリアル

## 概要

このリポジトリは、Microsoft Agent Framework と Model Context Protocol (MCP) を使用した AI エージェント開発の実践的なチュートリアルを提供します。ローカル LLM(Ollama)や Azure OpenAI を利用した AI エージェントの作成から、MCP サーバーの実装、そしてエージェントと MCP サーバーの統合まで、段階的に学習できます。

>このサンプルコードとチュートリアルは、執筆時点のプレビュー版の Microsoft Agent Framework が基になっています。
>Microsoft Agent Framework は、今後のバージョンアップで内容が変わる可能性があります。

## 特徴

- **.NET ベースの実装** - C# と .NET を使用した実装例
- **複数の LLM 対応** - ローカル LLM (Ollama) と Azure OpenAI の両方に対応
- **MCP サーバーの実装** - STDIO と SSE の両方のトランスポート方式をサポート
- **段階的な学習** - 基礎から応用まで、ステップバイステップで学習
- **実践的なサンプル** - すぐに動かせる実装例を複数提供
- **日本語ドキュメント** - すべてのチュートリアルを日本語で提供

## 内容一覧

### 0. 準備

- [インストール](./Documents/tutorial.0.0.md) - .NET、Node.js、Ollama のセットアップ
- [Azure OpenAI](./Documents/tutorial.0.1.md) - Azure OpenAI の設定

### 1. AI エージェントの作成 (LLM 利用)

- [ローカル LLM の利用](./Documents/tutorial.1.1.md) - Ollama を使った AI エージェントの作成
- [Azure OpenAI の利用](./Documents/tutorial.1.2.md) - Azure OpenAI を使った AI エージェントの作成
- [複数ターンのチャット](./Documents/tutorial.1.3.md) - 会話履歴を保持したチャット機能の実装

### 2. MCP サーバーの作成

- [MCP サーバー (STDIO) の作成](./Documents/tutorial.2.1.md) - 標準入出力を使った MCP サーバーの実装
- [MCP サーバー (SSE) の作成](./Documents/tutorial.2.2.md) - Server-Sent Events を使った MCP サーバーの実装
- [MCP サーバーの動作確認](./Documents/tutorial.2.3.md) - MCP サーバーの動作確認手順

### 3. AI エージェントでの MCP サーバーの利用

- [MCP サーバーの利用 (単数)](./Documents/tutorial.3.1.md) - 単一の MCP サーバーとの連携
- [MCP サーバーの利用 (複数)](./Documents/tutorial.3.2.md) - 複数の MCP サーバーとの連携

## チュートリアルの実施方法

### 前提条件

以下のツールがインストールされている必要があります:

- .NET 8.0 以上 (推奨: .NET 9.0)
- Node.js (v24.0 以上推奨)
- Ollama (ローカル LLM を使用する場合)
- Visual Studio と Visual Studio Code

### 実施手順

1. **リポジトリのクローン**

   ```bash
   git clone https://github.com/Fujiwo/Shos.AIAgentSample.git
   cd Shos.AIAgentSample
   ```

2. **必要なツールのインストール**

   [インストールガイド](./Documents/tutorial.0.0.md)に従って、必要なツールをインストールしてください。

3. **チュートリアルの実施**

   [チュートリアルメインページ](./Documents/tutorial.md)から、順番にチュートリアルを進めてください。各チュートリアルは独立して実施できますが、順番に進めることを推奨します。

4. **サンプルプロジェクトの実行**

   各プロジェクトは独立して実行できます:

   ```bash
   # AI エージェントの例
   cd FCAIAgent1
   dotnet run

   # MCP サーバーの例
   cd McpServer.Con
   dotnet run
   ```

## プロジェクト構成

- **FCAIAgent1-5** - AI エージェントのサンプル実装(段階的に機能を追加)
- **McpServer.Con** - STDIO を使用した MCP サーバーの実装
- **McpServer.Sse** - Server-Sent Events を使用した MCP サーバーの実装
- **Documents** - チュートリアルドキュメントと画像

## 実施環境

### オペレーティングシステム

- Windows 11
- macOS (一部機能)
- Linux (一部機能)

### 言語とランタイム

- **言語**: C# 12.0
- **ランタイム**: .NET 8.0 以上(推奨: .NET 9.0)
- **IDE**: Visual Studio 2022 17.12 以降、または Visual Studio Code

### 主要な依存パッケージ

- `Microsoft.Agents.AI` (プレリリース版)
- `Microsoft.Extensions.AI`
- `ModelContextProtocol` (プレリリース版)
- `OllamaSharp` - ローカル LLM との通信
- `Azure.AI.OpenAI` - Azure OpenAI との通信

## 技術用語の補足

- **AI エージェント**: LLM (大規模言語モデル) を活用して、ユーザーの要求に応じた処理を自律的に実行するソフトウェアコンポーネント
- **LLM (Large Language Model)**: 大規模言語モデル。自然言語を理解し生成する AI モデル
- **MCP (Model Context Protocol)**: AI エージェントと外部ツールやデータソースを接続するための標準プロトコル
- **Ollama**: ローカル環境で大規模言語モデルを実行するためのツール
- **STDIO**: 標準入出力。プロセス間通信の基本的な方式
- **SSE (Server-Sent Events)**: サーバーからクライアントへの一方向リアルタイム通信プロトコル

## ライセンス

MIT License

Copyright (c) 2025 Fujio Kojima

詳細は [LICENSE.txt](./LICENSE.txt) を参照してください。

## 連絡先

- **作者**: Fujio Kojima
- **リポジトリ**: https://github.com/Fujiwo/Shos.AIAgentSample

## 謝辞

このプロジェクトは、Microsoft Agent Framework と Model Context Protocol を使用しています。これらの素晴らしいツールを提供してくださった開発者の皆様に感謝いたします。

---

**注意**: このリポジトリのコードはチュートリアル目的で提供されています。本番環境での使用前に、適切なセキュリティレビューと最適化を行ってください。