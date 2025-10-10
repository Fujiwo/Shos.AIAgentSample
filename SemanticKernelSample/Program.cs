// Import packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//1) パッケージのインポート
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Populate values from your OpenAI deployment
var modelId = "gpt-5-mini";

var endPoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
if (string.IsNullOrEmpty(endPoint)) {
    Console.WriteLine("Please set the AZURE_OPENAI_ENDPOINT environment variable.");
    return;
}

var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
if (string.IsNullOrEmpty(apiKey)) {
    Console.WriteLine("Please set the AZURE_OPENAI_API_KEY environment variable.");
    return;
}

// 2) AI サービスを追加する
// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endPoint, apiKey);

// 3) エンタープライズ サービスを追加する
// Add enterprise components
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// 4) カーネルをビルドしてサービスを取得する
// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// プラグインをカーネルに追加する
// Add a plugin (the LightsPlugin class is defined below)
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// 9) 計画
// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() {
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// 10) 呼び出し
// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do {
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);