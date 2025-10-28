## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ AIエージェントの作成 (LLM利用) - 複数ターンのチャット
![AIエージェントの作成 (LLM利用) - 複数ターンのチャット](./Images/tutorial_banners/tutorial_banner_13.png)

この手順では、複数ターンの会話を行えるようにエージェントを改良していきます。

○ Program.cs を下記のようにに書き換え

```csharp
// Program.cs
//
// 【概要】
// Microsoft.Agents.AI フレームワークを使用した、AI エージェントの実装例
// 指定されたチャットクライアント(Ollama / Azure OpenAI)を利用し、複数ターン対話を実行
//
// 【前提条件】
// - Ollama がインストールされ、http://localhost:11434 で起動していること
// - Ollama でモデル "gpt-oss:20b-cloud" が利用可能であること
// - Azure OpenAI が作成され、エンドポイントと API キーが取得できていること
//
// 【実行方法】
// dotnet run --project FCAIAgent3
//
// 【動作説明】
// 1. チャットクライアント(Ollama / Azure OpenAI)を生成
// 2. ChatClientAgent を作成(エージェント名と指示を設定)
// 3. AgentThread を使った複数ターン対話ループを実行

using System;
// Microsoft Agent Framework 用
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
// Ollama 用
using OllamaSharp;
// Azure OpenAI のクライアント用
using Azure;
using Azure.AI.OpenAI;

// 使用するチャットクライアント種別
const My.ChatClientType chatClientType = My.ChatClientType.AzureOpenAI;
using IChatClient       chatClient     = My.GetChatClient(chatClientType);

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = My.AgentName   ,
        Instructions = My.Instructions
    }
);

// 旧: ここから
//try {
//    // エージェントを実行して結果を表示する
//    AgentRunResponse response = await agent.RunAsync(My.UserPrompt);
//    Console.WriteLine(response.Text);
//} catch (Exception ex) {
//    Console.WriteLine($"Error running agent: {ex.Message}");
//}
// 旧: ここまで

// 新: ここから
// 複数ターンに対応するために AgentThread (会話の状態・履歴などを管理) を作成
AgentThread thread = agent.GetNewThread();

// システムメッセージを作成して最初に送信
ChatMessage systemMessage = new(ChatRole.System, My.SystemPrompt);
await My.RunAsync(agent, systemMessage, thread);

Console.WriteLine($"(Interactive chat started. Type '{My.ExitPrompt}' to quit.)\n");

// 対話ループ: ユーザー入力を受け取り exit で終了
for (; ;) {
    var (isValid, userMessage) = My.GetUserMessage();
    if (!isValid)
        break;
    await My.RunAsync(agent, userMessage, thread);
}
// 新: ここまで

// 上記コード中の型や定数、メソッドが自作のものかどうかを判別しやすくするためにクラスに格納
static class My
{
    // エージェント名と指示
    public const string AgentName    = "AIエージェント";
    public const string Instructions = "あなたはAIエージェントです";
    // 新: ここから エージェントのシステムロールに与える文脈的な指示
    public const string SystemPrompt = "あなたはAIエージェントです";
    public const string ExitPrompt   = "exit";
    // 新: ここまで
    // 旧: ユーザーからのプロンプトの例
    //const string UserPrompt   = "「AIエージェント」とはどのようなものですか?";

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

    // Azure OpenAI を使う場合のクライアント生成
    static IChatClient GetAzureOpenAIClient()
    {
        // 使用するモデルを指定
        const string deploymentName = "gpt-5-mini";
        var azureOpenAIEndPoint     = GetEndPoint();
        var openAIApiKey            = GetKey     ();
        var credential              = new AzureKeyCredential(openAIApiKey);

        var azureOpenAIClient       = new AzureOpenAIClient(new Uri(azureOpenAIEndPoint), credential);
        // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
        IChatClient chatClient      = azureOpenAIClient.GetChatClient(deploymentName)
                                                       .AsIChatClient()
                                                       .AsBuilder()
                                                       .UseFunctionInvocation() // ツール呼び出しを使う
                                                       .Build();
        return chatClient;

        static string GetEndPoint()
        {
            //const string AzureOpenAIEndpointEnvironmentVariable = "AZURE_OPENAI_ENDPOINT";
            //var azureOpenAIEndPoint = Environment.GetEnvironmentVariable(AzureOpenAIEndpointEnvironmentVariable);
            //if (string.IsNullOrEmpty(azureOpenAIEndPoint))
            //    throw new InvalidOperationException($"Please set the {AzureOpenAIEndpointEnvironmentVariable} environment variable.");
            //return azureOpenAIEndPoint;

            // 上記のように、セキュリティ上 Azure OpenAI のエンドポイントは環境変数から取得するのが望ましいが、ここではハードコードする
            // 例: 1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef
            return @"[Azure OpenAI のエンドポイント]";
        }

        static string GetKey()
        {
            //const string AzureOpenAIApiKeyEnvironmentVariable = "AZURE_OPENAI_API_KEY";
            //var openAIApiKey = Environment.GetEnvironmentVariable(AzureOpenAIApiKeyEnvironmentVariable);
            //if (string.IsNullOrEmpty(openAIApiKey))
            //    throw new InvalidOperationException($"Please set the {AzureOpenAIApiKeyEnvironmentVariable} environment variable.");
            //return openAIApiKey!;

            // 上記のように、セキュリティ上 Azure OpenAI の APIキーは環境変数から取得するのが望ましいが、ここではハードコードする
            //例: https://your-resource-name.openai.azure.com/
            return @"[Azure OpenAI の APIキー]";
        }
    }

    // ChatClientType に基づいて適切な IChatClient を返すファクトリ関数
    public static IChatClient GetChatClient(My.ChatClientType chatClientType)
        => chatClientType switch {
            My.ChatClientType.Ollama      => My.GetOllamaClient     (),
            My.ChatClientType.AzureOpenAI => My.GetAzureOpenAIClient(),
            _ => throw new NotSupportedException($"Chat client type '{chatClientType}' is not supported.")
        };

    // 新: ここから
    // コンソールからユーザー入力を読み取り ChatMessage を返す
    public static (bool isValid, ChatMessage userMessage) GetUserMessage()
    {
        var (isValid, userPrompt) = GetUserPrompt();
        return (isValid, new ChatMessage(ChatRole.User, userPrompt));

        static (bool isValid, string userPrompt) GetUserPrompt()
        {
            Console.Write("You: ");
            var userPrompt = Console.ReadLine();
            Console.WriteLine();

            return string.IsNullOrWhiteSpace(userPrompt) ||
                   string.Equals(userPrompt.Trim(), ExitPrompt, StringComparison.OrdinalIgnoreCase)
                ? (isValid: false, userPrompt: string.Empty)
                : (isValid: true , userPrompt: userPrompt! );
        }
    }

    // エージェントに ChatMessage を投げて応答を取得
    public static async Task RunAsync(AIAgent agent, ChatMessage chatMessage, AgentThread? thread = null)
    {
        try {
            var response = await agent.RunAsync(chatMessage, thread);
            Console.WriteLine($"Agent: {response.Text ?? string.Empty}\n");
        } catch (Exception ex) {
            Console.WriteLine($"Error running agent: {ex.Message}");
        }
    }
    // 新: ここまで

    // チャットクライアントの種別
    public enum ChatClientType
    {
        AzureOpenAI,
        Ollama
    }
}
```

○ 動作確認

```console
dotnet run
```

- 実行例

```console
Agent: こんにちは、AIエージェントです。どういったことでお手伝いしましょうか？

できること（例）
- 文章作成・校正（メール、報告書、スピーチ、ブログなど）
- 翻訳・言い換え（日本語?英語ほか）
- プログラミング支援（コード作成・デバッグ・説明）
- 企画・計画作成（スケジュール、ビジネス案、プレゼン構成）
- 調査・要約（論文・記事の要点抽出）
- 学習支援・問題解説（数学、物理、語学など）
- 相談・アイデア出し（ブレインストーミング、改善案）

依頼する際に教えてほしいこと
- 目的・用途、期待する出力形式（箇条書き、文章、コード等）
- 読者やトーン（ビジネス向け、カジュアルなど）
- 長さや締切、特に重視する点

まずは具体的な依頼内容を教えてください。

(Interactive chat started. Type 'exit' to quit.)

You: こんにちは。

Agent: こんにちは！どういったことでお手伝いしましょうか？気軽に相談内容を教えてください（例：文章の添削、翻訳、調べもの、プログラミングの相談など）。

You: exit
```

○ プロンプトを色々変更して実行

- 例.

```csharp
// エージェント名と指示
public const string AgentName    = "テキストベースRPG";
public const string Instructions = @"あなたは**テキストベースRPGのゲームマスター**です";
// 新: エージェントのシステムロールに与える文脈的な指示
public const string SystemPrompt = @"
あなたは**テキストベースRPGのゲームマスター**です。
舞台は「ドラゴンクエスト」風の世界――**アレフガルド**。
プレイヤーはホビットの冒険者。
あなたは老魔法使い**ガンダルフ**として同行し、長老口調で語ります。
---
### 🗺 基本ルール
* すべて日本語で進行。
* 各場面で3〜5個の**番号付きコマンド**を提示。
* ステータスや戦闘は**ドラクエ風ウィンドウ形式**で表示。
* 宿屋で回復、モンスターとターン制バトル。
* セーブ＆ロードは「復活の呪文（文字列）」で。
---
### ⚔ 出力例

#### 初回
＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
ガンダルフ：「おお……見知らぬホビットよ。
そなたの名を教えてくれぬかのう？」
＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

#### 以降
ガンダルフ：「{ガンダルフのセリフ}」

[ステータス]
{プレイヤー名} Lv{Lv} HP:{HP} MP:{MP} G:{Gold}

コマンド：
1. はなす
2. まわりをみる
3. たたかう
4. どうぐ
5. ふっかつのじゅもん
---

ゲーム開始時に、
ガンダルフが「あなたの名前は？」と尋ねて開始してください。
---
";
```
