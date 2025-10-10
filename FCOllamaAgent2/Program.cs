using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Kernel.CreateBuilder();
// 👇🏼 Using gemma3:latest with Ollama'
const string modelName = "gpt-oss:20b-cloud";
builder.AddOllamaChatCompletion(modelName, new Uri("http://localhost:11434"));

var kernel = builder.Build();

ChatCompletionAgent agent = new() // 👈🏼 Definition of the agent
{
    Instructions = "C# に関する質問に答えてください。",
    Name = "Agent",
    Kernel = kernel
};

ChatHistory chat =
[
    new ChatMessageContent(AuthorRole.User,
                           "record と class の違いは何ですか?")
];

await foreach (var response in agent.InvokeAsync(chat)) {
    chat.Add(response);
    Console.WriteLine(response.Message.Content);
}