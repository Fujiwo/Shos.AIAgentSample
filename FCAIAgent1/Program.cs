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
