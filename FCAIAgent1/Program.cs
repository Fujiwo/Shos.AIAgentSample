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
// Microsoft Agent Framework 用
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
// Ollama 用
using OllamaSharp;

// Ollama を使うためのクライアント生成
using IChatClient chatClient = My.GetOllamaClient();

// ChatClientAgent の作成 (Agent の名前やインストラクションを指定する)
AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = My.AgentName   ,
        Instructions = My.Instructions
    }
);

try {
    // エージェントを実行して結果を表示する
    AgentRunResponse response = await agent.RunAsync(My.UserPrompt);
    Console.WriteLine(response.Text);
} catch (Exception ex) {
    Console.WriteLine($"Error running agent: {ex.Message}");
}

// 上記コード中の型や定数、メソッドが自作のものかどうかを判別しやすくするためにクラスに格納
static class My
{
    // エージェント名と指示
    public const string AgentName    = "AIエージェント";
    public const string Instructions = "あなたはAIエージェントです";
    // ユーザーからのプロンプトの例
    public const string UserPrompt   = "「AIエージェント」とはどのようなものですか?";

    // Ollama を使う場合のクライアント生成(ローカルの Ollama サーバーに接続)
    public static IChatClient GetOllamaClient()
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
}
