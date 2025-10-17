## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントの作成 (LLM利用) - ローカルLLMの利用
![AIエージェントの作成 (LLM利用) - ローカルLLMの利用](./Images/tutorial_banner_11.png)

この手順では、ローカルLLM にプロンプトを投げて、返事を受け取るAIエージェントを作成します。

1.1 C#/.NET コンソール アプリケーションを作成

任意のフォルダーでソリューションを作成し、コンソール アプリケーションを作成する

>Visual Studio の代わりに Visual Studio Code でも開発できる。
>その場合は、Visual Studio Code に拡張機能の「C# Dev Kit」をインストールしておく

※ Windows のターミナルなどで

```console
cd [今回サンプルを作成するフォルダー]
```
(例. cd C:\source)

```console
md FCAIAgentSample
cd FCAIAgentSample
dotnet new sln
dotnet new console -n FCAIAgent
cd FCAIAgent
```

実行結果
```console
C:\>cd Source

C:\Source>md FCAIAgentSample

C:\Source>cd FCAIAgentSample

C:\Source\FCAIAgentSample>dotnet new sln
テンプレート "ソリューション ファイル" が正常に作成されました。


C:\Source\FCAIAgentSample>dotnet new console -n FCAIAgent
テンプレート "コンソール アプリ" が正常に作成されました。

作成後の操作を処理しています...
C:\Source\FCAIAgentSample\FCAIAgent\FCAIAgent.csproj を復元しています:
正常に復元されました。



C:\Source\FCAIAgentSample>cd FCAIAgent

C:\Source\FCAIAgentSample\FCAIAgent>
```

Microsoft Agent Framework パッケージをインストール

```console
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package OllamaSharp
```

Visual Studio の「ソリューション エクスプローラー」でソリューションを右クリックし、「追加」-「既存のプロジェクト」で、FCAIAgent プロジェクトを追加

Program.cs を下記に書き換え

```csharp
// Program.cs
using System;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OllamaSharp;

// AI エージェントの実行例
// - 指定されたチャットクライアント（Ollama）を作成
// - ChatClientAgent を作成して、簡単なプロンプトを投げる

// エージェント名と指示
const string agentName    = "AIエージェント";
const string instructions = "あなたはAIエージェントです";
// ユーザーからのプロンプトの例
const string userPrompt   = "「AIエージェント」とはどのようなものですか?";

// Ollama を使うためのクライアント生成
IChatClient chatClient = GetOllamaClient();

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = agentName,
        Instructions = instructions
    }
);

// エージェントを実行して結果を表示する
AgentRunResponse response = await agent.RunAsync(userPrompt);
Console.WriteLine(response.Text);

// Ollama を使う場合のクライアント生成（ローカルの Ollama サーバーに接続）
static IChatClient GetOllamaClient()
{
    var uri    = new Uri("http://localhost:11434");
    var ollama = new OllamaApiClient(uri);
    // 使用するモデルを指定
    ollama.SelectedModel = "gpt-oss:20b-cloud"; // ここでは実行速度の都合でクラウドのものを選択しているが、ローカルLLMの場合は "gemma3:latest" など

    // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
    IChatClient chatClient = ollama;
    chatClient = chatClient.AsBuilder()
                           .UseFunctionInvocation() // ツール呼び出しを使う
                           .Build();
    return chatClient;
}
```

Ollama が起動していることを確認して、動作確認

```console
dotnet run
```

AIエージェントとして、LLM からの応答が得られことを確認

実行例
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
