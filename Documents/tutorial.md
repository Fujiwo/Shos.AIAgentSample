
## 【C#/.NET】Microsoft Agent Framework による AIエージェント開発 チュートリアル

![AIエージェント](./Images/tutorial_banners/tutorial_banner_aiagent.png)

>このチュートリアルは執筆時点のプレビュー版の Microsoft Agent Framework が基になっています。
>Microsoft Agent Framework は、今後のバージョンアップで内容が変わる可能性があります。

## このチュートリアルについて

このチュートリアルでは、Microsoft Agent Framework と Model Context Protocol (MCP) を使用した AI エージェント開発を、C#/.NET で段階的に学習できます。

- ローカル LLM(Ollama) や Azure OpenAI を利用した AI エージェントの作成
- MCP サーバーの実装
- エージェントと MCP サーバーの統合

## 前提知識

- プログラミング言語 C#

## 学習の進め方

各ステップは前のステップの知識を前提としているため、**順番に進めることを強く推奨**します。

1. **準備**から始めて、必要なツール等をインストール

1. 2. 資料「**[2025010.AIエージェント開発ハンズオンセミナー](./2025010.AIエージェント開発ハンズオンセミナー.pdf)**」で AI エージェントの基本を理解

- LLM (Large Language Model: 大規模言語モデル)
- AIエージェント
- MCP (Model Context Protocol)
- Microsoft Agent Framework

3. AIエージェントの作成で実践的なスキルを習得
 - **AIエージェントの作成 (LLM利用)**
 - **MCP サーバーの作成**
 - **AIエージェントでの MCP サーバーの利用**

4. **後片付け**で Azure リソースをクリーンアップ

各チュートリアルには、コード例、実行結果、スクリーンショットが含まれており、手順に従って進めることで実際に動作するサンプルを作成できます。

コードでは、前のステップからの変更箇所が下記のように示されています:

```csharp
// 旧: using IChatClient chatClient = GetOllamaClient();
// 新: ここから
// 使用するチャットクライアント種別
const My.ChatClientType chatClientType = My.ChatClientType.AzureOpenAI;
using IChatClient       chatClient     = My.GetChatClient(chatClientType);
// 新: ここまで
```

新たなステップでは、「旧」の部分を「新」に書き換えてください。

## チュートリアル

 0. 準備
  - 0\.1 [インストール](./tutorial.0.1.md)
	- .NET、Node.js、Ollama のセットアップ
  - 0\.2 [Azure OpenAI](./tutorial.0.2.md)
	- Azure OpenAI の設定
1. AIエージェントの作成 (LLM利用)
  - 1\.1 [ローカルLLMの利用](./tutorial.1.1.md)
	- Ollama を使った AI エージェントの作成
  - 1\.2 [Azure OpenAIの利用](./tutorial.1.2.md)
	- Azure OpenAI を使った AI エージェントの作成
  - 1\.3 [複数ターンのチャット](./tutorial.1.3.md)
	- 会話履歴を保持したチャット機能の実装
2. MCP サーバーの作成
  - 2\.1 [MCP サーバー (STDIO) の作成](./tutorial.2.1.md)
	- 標準入出力を使った MCP サーバーの実装
  - 2\.2 [MCP サーバー (SSE) の作成](./tutorial.2.2.md)
	- Server-Sent Events を使った MCP サーバーの実装
  - 2\.3 [MCP サーバーの動作確認](./tutorial.2.3.md)
	- MCP サーバーの動作確認手順
3. AIエージェントでの MCP サーバーの利用
  - 3\.1 [MCP サーバーの利用 (単数)](./tutorial.3.1.md)
	- 単一の MCP サーバーとの連携
  - 3\.2 [MCP サーバーの利用 (複数)](./tutorial.3.2.md)
	- 複数の MCP サーバーとの連携
4. [後片付け](./tutorial.4.1.md)
