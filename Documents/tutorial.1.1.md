## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントの作成 (LLM利用) - ローカルLLMの利用
![AIエージェントの作成 (LLM利用) - ローカルLLMの利用](./Images/tutorial_banner_11.png)

この手順では、ローカルLLM にプロンプトを投げて、返事を受け取るAIエージェントを作成します。

○ C#/.NET コンソール アプリケーションを作成

- 任意のフォルダーでソリューション \"FCAIAgentSample\" を作成する

>Visual Studio でも Visual Studio Code でも開発できる。<br>
>Visual Studio Code の場合は、拡張機能の「C# Dev Kit」をインストールしておく。
>
>参考: [Visual Studio Code のインストールと構成 \- Training \| Microsoft Learn](https://learn.microsoft.com/ja-jp/training/modules/install-configure-visual-studio-code/)

Windows のターミナルなどで以下を実行

```console
cd [予め用意した今回サンプルを作成するフォルダー 例. cd \source]
md FCAIAgentSample
cd FCAIAgentSample
dotnet new sln
```

- 実行結果
```console
C:\>cd \Source

C:\Source>md FCAIAgentSample

C:\Source>cd FCAIAgentSample

C:\Source\FCAIAgentSample>dotnet new sln
テンプレート "ソリューション ファイル" が正常に作成されました。
```

- コンソール アプリケーション \"FCAIAgent\" を作成

```console
dotnet new console -n FCAIAgent
dotnet sln add ./FCAIAgent/FCAIAgent.csproj
cd FCAIAgent
```

- 実行結果
```console
C:\Source\FCAIAgentSample>dotnet new console -n FCAIAgent
テンプレート "コンソール アプリ" が正常に作成されました。

作成後の操作を処理しています...
C:\Source\FCAIAgentSample\FCAIAgent\FCAIAgent.csproj を復元しています:
正常に復元されました。

C:\Source\FCAIAgentSample>dotnet sln add ./FCAIAgent/FCAIAgent.csproj
プロジェクト `FCAIAgent\FCAIAgent.csproj` をソリューションに追加しました。

C:\Source\FCAIAgentSample>cd FCAIAgent

C:\Source\FCAIAgentSample\FCAIAgent>
```

- Microsoft Agent Framework パッケージをインストール

```console
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package OllamaSharp
```

○ Program.cs を下記に書き換え

```csharp
// Program.cs
//
// 【概要】
// Microsoft.Agents.AI フレームワークを使用した、最もシンプルな AI エージェントの実装例
//
// 【前提条件】
// - Ollama がインストールされ、http://localhost:11434 で起動していること
// - Ollama でモデル "gpt-oss:20b-cloud" が利用可能であること
//
// 【実行方法】
// dotnet run --project FCAIAgent1
//
// 【動作説明】
// 1. Ollama クライアントを生成
// 2. ChatClientAgent を作成(エージェント名と指示を設定)
// 3. ユーザープロンプトを送信して応答を取得
// 4. 応答内容をコンソールに出力

using System;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OllamaSharp;

// エージェント名と指示
const string agentName    = "AIエージェント";
const string instructions = "あなたはAIエージェントです";
// ユーザーからのプロンプトの例
const string userPrompt   = "「AIエージェント」とはどのようなものですか?";

// Ollama を使うためのクライアント生成
using IChatClient chatClient = GetOllamaClient();

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = agentName,
        Instructions = instructions
    }
);

try {
    // エージェントを実行して結果を表示する
    AgentRunResponse response = await agent.RunAsync(userPrompt);
    Console.WriteLine(response.Text);
} catch (Exception ex) {
    Console.WriteLine($"Error running agent: {ex.Message}");
}

// Ollama を使う場合のクライアント生成(ローカルの Ollama サーバーに接続)
static IChatClient GetOllamaClient()
{
    var uri    = new Uri("http://localhost:11434");
    var ollama = new OllamaApiClient(uri);
    // 使用するモデルを指定
    // クラウドベースのモデルを使用(実行速度の向上のため)
    // ローカル LLM を使用する場合は "gemma3:latest" などに変更してください
    ollama.SelectedModel = "gpt-oss:20b-cloud";

    // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
    IChatClient chatClient = ollama;
    chatClient = chatClient.AsBuilder()
                           .UseFunctionInvocation() // ツール呼び出しを使う
                           .Build();
    return chatClient;
}
```

○ Ollama が起動していることを確認して、動作確認

```console
dotnet run
```

AIエージェントとして、LLM からの応答が得られことを確認

- 実行例
```console
## 「AIエージェント」とは

### 1. 基本定義
**AIエージェント** とは、環境（世界）を「知覚」し、その知覚をもとに「意思決定」し、最後に「行動」を実行する自律的な存在です。
- **知覚（Perception）**：センサーや入力データ（画像、音声、テキスト、数値データなど）で環境情報を取得
- **意思決定（Decision/Planning）**：入力を解釈し、目標を達成するための最適な行動を選択
- **行動（Actuation/Action）**：選択した行動を環境に対して実行する（アクション・メカニズム）

つまり、単に予測や分類を行う「モデル」ではなく、**“できること”** を行う「実践的な主体」なのです。
……
```
