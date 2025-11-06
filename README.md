# Shos.AIAgentSample

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120)](https://docs.microsoft.com/dotnet/csharp/)

AIエージェント開発ハンズオンセミナー (開発者向け) のサンプルコードとチュートリアル

**リポジトリ**: https://github.com/Fujiwo/Shos.AIAgentSample

## 目次

- [概要](#概要)
- [特徴](#特徴)
- [クイックスタート](#クイックスタート)
- [内容一覧](#内容一覧)
- [チュートリアルの実施方法](#チュートリアルの実施方法)
  - [前提条件](#前提条件)
  - [学習の進め方](#学習の進め方)
  - [学習のヒント](#学習のヒント)
- [プロジェクト構成](#プロジェクト構成)
- [ファイル・ディレクトリ構成](#ファイルディレクトリ構成)
- [実施環境](#実施環境)
- [技術用語の補足](#技術用語の補足)
- [トラブルシューティング](#トラブルシューティング)
- [ライセンス](#ライセンス)
- [作者](#作者)
- [謝辞](#謝辞)
- [貢献](#貢献)

## 概要

このリポジトリは、Microsoft Agent Framework と Model Context Protocol (MCP) を使用した AI エージェント開発の実践的なチュートリアルを提供します。ローカル LLM(Ollama)や Azure OpenAI を利用した AI エージェントの作成から、MCP サーバーの実装、そしてエージェントと MCP サーバーの統合まで、段階的に学習できます。

>このサンプルコードとチュートリアルは、執筆時点のプレビュー版の Microsoft Agent Framework が基になっています。
>Microsoft Agent Framework は、今後のバージョンアップで内容が変わる可能性があります。

## 特徴

- **.NET ベースの実装** - C# と .NET を使用した実装例
- **複数の LLM 対応** - ローカル LLM (Ollama) と Azure OpenAI の両方に対応
- **MCP サーバーの実装** - STDIO と SSE の両方のトランスポート方式をサポート
- **段階的な学習** - 基礎から応用まで、ステップバイステップで学習
- **実践的なサンプル** - コンソールアプリと Web アプリの両方を提供
- **日本語ドキュメント** - すべてのチュートリアルを日本語で提供

## クイックスタート

最も簡単に試すには：

1. リポジトリをクローン
2. [Ollama](https://ollama.com/) をインストール（ローカル LLM を使用）
3. Ollama で `llama3.2` モデルをダウンロード: `ollama pull llama3.2`
4. Visual Studio で `Shos.AIAgentSample.sln` を開く
5. `FCAIAgent1` プロジェクトを実行

詳細は[チュートリアル](#チュートリアルの実施方法)を参照してください。

## 内容一覧

### 0. 準備

- [インストール](./Documents/tutorial.0.1.md) - .NET、Node.js、Ollama のセットアップ
- [Azure OpenAI](./Documents/tutorial.0.2.md) - Azure OpenAI の設定

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

### 4. [後片付け](./Documents/tutorial.4.1.md)

## チュートリアルの実施方法

このチュートリアルは、AI エージェント開発の基礎から応用まで、段階的に学習できるように設計されています。

### 前提条件

#### 必須要件

以下のツールがインストールされている必要があります:

- **.NET 8.0 以上** (推奨: .NET 9.0)
  - [インストールガイド](./Documents/tutorial.0.1.md)を参照
- **Visual Studio 2022 (17.12 以降)** または **Visual Studio Code**
  - チュートリアルは Visual Studio を前提としていますが、Visual Studio Code でも実施可能です
- **Node.js** (v24.0 以上推奨)
  - MCP サーバーの動作確認ツールに必要
  - [インストールガイド](./Documents/tutorial.0.1.md)を参照

#### 推奨される参考資料

事前学習として以下の資料が役立ちます:
- [AIエージェント開発ハンズオンセミナー (PDF)](./Documents/2025010.AIエージェント開発ハンズオンセミナー.pdf) - セミナー資料（前提知識を含む）

#### 選択要件

使用する LLM に応じて、以下のいずれかが必要です:

- **Ollama** - ローカル LLM を使用する場合
  - 無料で利用可能
  - インターネット接続不要で動作
  - [インストールガイド](./Documents/tutorial.0.1.md)を参照
  
- **Azure OpenAI アカウント** - Azure OpenAI を使用する場合
  - Microsoft Azure アカウントが必要
  - 課金が発生する可能性があります
  - [設定ガイド](./Documents/tutorial.0.2.md)を参照

#### 推奨される前提知識

- C# プログラミングの基礎
- .NET アプリケーションの基本的な開発経験
- コマンドライン操作の基礎知識

### 学習の進め方

#### ステップ 1: 環境準備

1. **リポジトリのクローン**

   ```bash
   git clone https://github.com/Fujiwo/Shos.AIAgentSample.git
   cd Shos.AIAgentSample
   ```

2. **必要なツールのインストール**

   [インストールガイド](./Documents/tutorial.0.1.md)に従って、必要なツールをインストールしてください。

3. **ソリューションを開く**

   Visual Studio で `Shos.AIAgentSample.sln` を開きます。

#### ステップ 2: チュートリアルの実施

推奨される学習順序：

1. **準備編**（所要時間：30分〜1時間）
   - [0.1 インストール](./Documents/tutorial.0.1.md) - 開発環境のセットアップ
   - [0.2 Azure OpenAI](./Documents/tutorial.0.2.md) - Azure OpenAI の設定（Azure OpenAI を使用する場合）

2. **基礎編**（所要時間：1〜2時間）
   - [1.1 ローカル LLM の利用](./Documents/tutorial.1.1.md) - 最初の AI エージェントを作成
   - [1.2 Azure OpenAI の利用](./Documents/tutorial.1.2.md) - クラウド LLM への接続
   - [1.3 複数ターンのチャット](./Documents/tutorial.1.3.md) - 対話型エージェントの実装

3. **MCP サーバー編**（所要時間：1〜2時間）
   - [2.1 MCP サーバー (STDIO) の作成](./Documents/tutorial.2.1.md) - 最初の MCP サーバー
   - [2.2 MCP サーバー (SSE) の作成](./Documents/tutorial.2.2.md) - HTTP/SSE サーバー
   - [2.3 MCP サーバーの動作確認](./Documents/tutorial.2.3.md) - サーバーの検証

4. **応用編**（所要時間：1〜2時間）
   - [3.1 MCP サーバーの利用（単数）](./Documents/tutorial.3.1.md) - ツールとエージェントの統合
   - [3.2 MCP サーバーの利用（複数）](./Documents/tutorial.3.2.md) - 複数ツールの活用

5. **後片付け**
   - [4.1 後片付け](./Documents/tutorial.4.1.md) - Azure リソースのクリーンアップ

#### ステップ 3: サンプルプロジェクトの実行

各プロジェクトは独立して実行できます。チュートリアルに従って作成することもできますが、既存のサンプルプロジェクトを参照・実行することもできます。

> **注意**: 既存のサンプルを実行する場合、API キーやエンドポイント、プロジェクトのパスなどを、環境に合わせてソースコード内に追記する必要があります。

**コンソールアプリケーションの実行例:**

```bash
# 基本的な AI エージェント (FCAIAgent1〜5)
cd FCAIAgent1
dotnet run

# MCP サーバーの例
cd McpServer.Con
dotnet run
```

**Web アプリケーションの実行例:**

```bash
# Web チャットアプリケーション (FCAIChat1〜5)
cd FCAIChat1
dotnet run

# ブラウザで https://localhost:5001 にアクセス
```

### 学習のヒント

- **順番に進める**: 各チュートリアルは前のステップの知識を前提としているため、順番に進めることを強く推奨します
- **コードを理解する**: 単にコピー＆ペーストするのではなく、各コードの意味を理解しながら進めてください
- **実験する**: サンプルコードを改変して、動作を確認することで理解が深まります
- **エラーに慣れる**: エラーメッセージを読み、問題を解決するプロセスも重要な学習です
- **コメントを読む**: サンプルコードには詳細なコメントが付いているので、よく読んでください

## プロジェクト構成

このリポジトリには、段階的に学習できる複数のサンプルプロジェクトが含まれています。

### AI エージェントプロジェクト

各プロジェクトは前のプロジェクトの機能を拡張していく形になっています：

#### コンソールアプリケーション

- **FCAIAgent1** - 基本的な AI エージェントの実装
  - Ollama を使用したローカル LLM との通信
  - シンプルな1回のプロンプトと応答の実装
  
- **FCAIAgent2** - LLM の選択機能を追加
  - Ollama と Azure OpenAI の両方に対応
  - チャットクライアントの切り替え機能
  
- **FCAIAgent3** - 複数ターンの対話機能を実装
  - 会話履歴を保持したチャット機能
  - AgentThread を使用した対話セッション管理
  
- **FCAIAgent4** - MCP サーバーとの連携（単一サーバー）
  - STDIO トランスポートを使用した MCP サーバー接続
  - 外部ツール（時刻取得など）の利用
  
- **FCAIAgent5** - 複数の MCP サーバーとの連携
  - 複数の MCP サーバー（時刻、天気、ファイルシステム）の同時利用
  - より実践的なツール統合の例

- **FCAIAgent** - 最終形態 (FCAIAgent5と同じ実装)

#### Web アプリケーション (ASP.NET Core)

- **FCAIChat1** - 基本的な Web チャットインターフェース
  - ASP.NET Core MVC を使用した Web UI
  - ローカル LLM (Ollama) との対話
  
- **FCAIChat2** - LLM 選択機能を追加した Web チャット
  - Ollama と Azure OpenAI の切り替え機能
  - Web ベースの UI で LLM を選択可能
  
- **FCAIChat3** - 複数ターン対話を実装した Web チャット
  - セッション管理による会話履歴の保持
  - 連続的な対話のサポート
  
- **FCAIChat4** - MCP サーバー連携 Web チャット（単一サーバー）
  - Web インターフェースから MCP ツールを利用
  - ユーザーフレンドリーな対話型 AI 体験
  
- **FCAIChat5** - 複数 MCP サーバー連携 Web チャット
  - 複数のツールを統合した実践的な Web アプリケーション
  - エンドユーザー向けの完全な AI エージェント体験

### MCP サーバープロジェクト

- **McpServer.Con** - STDIO トランスポートを使用した MCP サーバー
  - 標準入出力を通じた通信
  - 時刻取得ツールの実装例
  - ローカルプロセス間通信向け
  
- **McpServer.Sse** - HTTP/SSE トランスポートを使用した MCP サーバー
  - Server-Sent Events によるリアルタイム通信
  - 天気予報ツールの実装例
  - ネットワーク経由での利用が可能

### ドキュメント

- **Documents** - チュートリアルドキュメントと画像
  - 各ステップの詳細な説明
  - スクリーンショットによる視覚的なガイド
  - PDF およびスライド形式のセミナー資料

### 設定ファイル

- **.mcp.json** - ルートディレクトリの MCP サーバー設定（サンプル）
- **.vscode/mcp.json** - Visual Studio Code 用の MCP サーバー設定
- **Shos.AIAgentSample.sln** - Visual Studio ソリューションファイル

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
- **Microsoft Agent Framework**: Microsoft が提供する AI エージェント開発用のフレームワーク（プレビュー版）
- **IChatClient**: .NET の AI 拡張における、チャット機能を提供する標準インターフェース
- **AgentThread**: 複数ターンの対話を管理するためのオブジェクト

## ファイル・ディレクトリ構成

```
Shos.AIAgentSample/
├── .mcp.json                    # MCP サーバー設定（ルート、サンプル）
├── .vscode/
│   └── mcp.json                # Visual Studio Code 用 MCP 設定
├── .github/                     # GitHub Actions とエージェント設定
├── Documents/                   # チュートリアルとリソース
│   ├── Images/                 # チュートリアル用画像
│   ├── tutorial.md             # チュートリアルメインページ
│   ├── tutorial.0.1.md         # インストールガイド
│   ├── tutorial.0.2.md         # Azure OpenAI セットアップ
│   ├── tutorial.1.1.md         # ローカル LLM の利用
│   ├── tutorial.1.2.md         # Azure OpenAI の利用
│   ├── tutorial.1.3.md         # 複数ターンのチャット
│   ├── tutorial.2.1.md         # MCP サーバー (STDIO)
│   ├── tutorial.2.2.md         # MCP サーバー (SSE)
│   ├── tutorial.2.3.md         # MCP サーバーの動作確認
│   ├── tutorial.3.1.md         # MCP サーバー利用（単数）
│   ├── tutorial.3.2.md         # MCP サーバー利用（複数）
│   ├── tutorial.4.1.md         # 後片付け
│   ├── aiagentlog.md           # AI エージェント開発ログ
│   ├── maf.md                  # Microsoft Agent Framework 情報
│   ├── 2025010.AIエージェント開発ハンズオンセミナー.pptx
│   └── 2025010.AIエージェント開発ハンズオンセミナー.pdf
├── FCAIAgent/                   # 最終形態の AI エージェント (コンソール)
│   ├── Program.cs
│   └── FCAIAgent.csproj
├── FCAIAgent1/                  # 基本的な AI エージェント
│   ├── Program.cs              # メインプログラム
│   └── FCAIAgent1.csproj       # プロジェクトファイル
├── FCAIAgent2/                  # LLM 選択機能追加
│   ├── Program.cs
│   └── FCAIAgent2.csproj
├── FCAIAgent3/                  # 複数ターン対話
│   ├── Program.cs
│   └── FCAIAgent3.csproj
├── FCAIAgent4/                  # MCP サーバー連携（単数）
│   ├── Program.cs
│   └── FCAIAgent4.csproj
├── FCAIAgent5/                  # MCP サーバー連携（複数）
│   ├── Program.cs
│   └── FCAIAgent5.csproj
├── FCAIChat1/                   # 基本的な Web チャット
│   ├── Controllers/            # MVC コントローラー
│   ├── Models/                 # データモデル
│   ├── Views/                  # Razor ビュー
│   ├── wwwroot/                # 静的ファイル
│   └── FCAIChat1.csproj
├── FCAIChat2/                   # LLM 選択機能付き Web チャット
│   └── FCAIChat2.csproj
├── FCAIChat3/                   # 複数ターン対話 Web チャット
│   └── FCAIChat3.csproj
├── FCAIChat4/                   # MCP 連携 Web チャット（単数）
│   └── FCAIChat4.csproj
├── FCAIChat5/                   # MCP 連携 Web チャット（複数）
│   └── FCAIChat5.csproj
├── McpServer.Con/               # STDIO MCP サーバー
│   ├── Program.cs              # 時刻取得ツール実装
│   └── McpServer.Con.csproj
├── McpServer.Sse/               # HTTP/SSE MCP サーバー
│   ├── Program.cs              # 天気予報ツール実装
│   └── McpServer.Sse.csproj
├── Reviews/                     # コードレビュー記録
├── Shos.AIAgentSample.sln      # Visual Studio ソリューション
├── LICENSE.txt                  # ライセンス情報（MIT License）
└── README.md                    # このファイル
```

## トラブルシューティング

### よくある問題と解決方法

<details>
<summary><strong>ビルドエラーが発生する</strong></summary>

- **.NET SDK のバージョン確認**
  ```bash
  dotnet --version
  ```
  .NET 8.0 以上（推奨: .NET 9.0）がインストールされているか確認してください。

- **パッケージの復元**
  ```bash
  dotnet restore
  ```
  
- **プロジェクトのクリーンとリビルド**
  ```bash
  dotnet clean
  dotnet build
  ```
</details>

<details>
<summary><strong>Ollama に接続できない</strong></summary>

- **Ollama が起動しているか確認**
  - Windows: タスクトレイに Ollama アイコンが表示されているか確認
  - ブラウザで `http://localhost:11434` にアクセスして動作確認

- **モデルがダウンロードされているか確認**
  ```bash
  ollama list
  ```
  
- **必要なモデルのダウンロード**
  ```bash
  ollama pull llama3.2
  ```
</details>

<details>
<summary><strong>Azure OpenAI に接続できない</strong></summary>

- **API キーとエンドポイントの確認**
  - Azure Portal で正しい API キーとエンドポイントを取得しているか確認
  - コード内の設定値が正しいか確認

- **デプロイ名の確認**
  - Azure OpenAI Studio でデプロイしたモデル名が正しいか確認
  
- **ネットワーク接続の確認**
  - ファイアウォールやプロキシ設定を確認
</details>

<details>
<summary><strong>MCP サーバーが見つからない</strong></summary>

- **.mcp.json のパス確認**
  - `.mcp.json` 内のパスが環境に合っているか確認
  - Windows の場合: バックスラッシュ (`\`) を使用
  - Linux/Mac の場合: スラッシュ (`/`) を使用

- **MCP サーバーのビルド確認**
  ```bash
  cd McpServer.Con
  dotnet build
  ```
</details>

<details>
<summary><strong>Web アプリケーションが起動しない</strong></summary>

- **ポートの競合確認**
  - 他のアプリケーションが同じポートを使用していないか確認
  - `launchSettings.json` でポート番号を変更可能

- **HTTPS 証明書の問題**
  ```bash
  dotnet dev-certs https --trust
  ```
</details>

問題が解決しない場合は、[Issues](https://github.com/Fujiwo/Shos.AIAgentSample/issues) で質問してください。

## ライセンス

MIT License

Copyright (c) 2025 Fujio Kojima

詳細は [LICENSE.txt](./LICENSE.txt) を参照してください。

## 作者

**小島 富治雄 (Fujio Kojima)** - ソフトウェア開発エンジニア

### 経歴・実績
- Microsoft MVP for Development Tools - Visual C# (Jul. 2005 - Dec. 2014)
- Microsoft MVP for .NET (Jan. 2015 - Oct. 2015)
- Microsoft MVP for Visual Studio and Development Technologies (Nov. 2015 - Jun. 2018)
- Microsoft MVP for Developer Technologies (Nov. 2018 - Jun. 2026)

### 連絡先・リンク
- **GitHub**: [Fujiwo](https://github.com/Fujiwo)
- **MVP Profile**: [Microsoft MVP](https://mvp.microsoft.com/en-us/PublicProfile/21482)
- **ブログ**: [http://wp.shos.info](http://wp.shos.info) (Japanese)
- **Web サイト**: [http://www.shos.info](http://www.shos.info) (Japanese)
- **X (Twitter)**: [@Fujiwo](https://x.com/Fujiwo)
- **Instagram**: [@fujiwo](https://www.instagram.com/fujiwo/)

## 謝辞

このプロジェクトは、以下のオープンソースプロジェクトと技術を使用しています:

- [Microsoft Agent Framework](https://github.com/microsoft/agents) - AI エージェント開発フレームワーク
- [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) - AI エージェントとツール連携の標準プロトコル
- [Ollama](https://ollama.com/) - ローカル LLM 実行環境
- [Azure OpenAI Service](https://azure.microsoft.com/services/cognitive-services/openai-service/) - クラウド LLM サービス

これらの素晴らしいツールとフレームワークを提供してくださった開発者とコミュニティの皆様に感謝いたします。

## 貢献

バグ報告、機能要望、プルリクエストを歓迎します。

1. このリポジトリをフォーク
2. フィーチャーブランチを作成 (`git checkout -b feature/amazing-feature`)
3. 変更をコミット (`git commit -m 'Add some amazing feature'`)
4. ブランチにプッシュ (`git push origin feature/amazing-feature`)
5. プルリクエストを作成

詳細は [Issues](https://github.com/Fujiwo/Shos.AIAgentSample/issues) または [Pull Requests](https://github.com/Fujiwo/Shos.AIAgentSample/pulls) をご覧ください。

---

## 重要な注意事項

> **⚠️ このリポジトリのコードは学習・チュートリアル目的で提供されています。**
> 
> - プレビュー版の Microsoft Agent Framework を使用しています
> - 本番環境での使用前に、適切なセキュリティレビューと最適化を行ってください
> - API キーやシークレットは環境変数や安全な設定管理ツールを使用してください
> - Microsoft Agent Framework は今後のバージョンアップで仕様が変わる可能性があります

---

**Last Updated**: 2025-01  
**Repository**: https://github.com/Fujiwo/Shos.AIAgentSample
