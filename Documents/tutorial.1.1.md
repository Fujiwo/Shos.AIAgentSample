## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントの作成 (LLM利用) - ローカルLLMの利用

1.1 C#/.NET コンソール アプリケーションを作成

Visual Studio で空のソリューションを作成し、
メニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」

```console
dotnet new console -n FCAIAgent
cd FCAIAgent
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
    ollama.SelectedModel = "gpt-oss:20b-cloud";

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
