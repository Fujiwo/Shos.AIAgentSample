using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Kernel.CreateBuilder();
// 👇🏼 Using gemma3:latest with Ollama
const string modelName = "gemma3:latest";
builder.AddOllamaChatCompletion(modelName, new Uri("http://localhost:11434"));

var kernel = builder.Build();

ChatCompletionAgent agent = new() // 👈🏼 Definition of the agent
{
    Instructions = "Answer.",
    Name = "Agent",
    Kernel = kernel
};

ChatHistory chat =
[
    new ChatMessageContent(AuthorRole.User,
                           "Hi.")
];

await foreach (var response in agent.InvokeAsync(chat)) {
    chat.Add(response);
    Console.WriteLine(response.Message.Content);
}