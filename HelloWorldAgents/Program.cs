using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

const ChatClientType chatClientType = ChatClientType.GitHub;
var chatClient = GetChatClient(chatClientType);

//const string agentName            = "RPGマスター";
//const string instructions         = "あなたは 世界最高のスペックを持つテキストベースRPGマスターAI です。";
//const string systemPromptFileName = "SystemPrompt.md";

const string agentName = "AIエージェント";
const string instructions = "あなたはAIエージェントです。";
const string systemPromptFileName = "SystemPrompt0.md";

var (mcpClient1, tools1) = await GetMcpServerTools(McpServer.Time      );
var (mcpClient2, tools2) = await GetMcpServerTools(McpServer.Weather   );
var (mcpClient3, tools3) = await GetMcpServerTools(McpServer.FileSystem);
var (mcpClient4, tools4) = await GetMcpServerTools(McpServer.GitHub    );
McpClient[] mcpClients = [mcpClient1, mcpClient2, mcpClient3, mcpClient4];
IEnumerable<McpClientTool> tools = [..tools1, ..tools2, ..tools3, ..tools4];

AIAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions {
        Name         = agentName,
        Instructions = instructions,
        ChatOptions  = new ChatOptions { Tools = tools.Cast<AITool>().ToList() }
    }
);

AgentThread thread = agent.GetNewThread();

ChatMessage systemMessage = GetSystemMessage();
await RunAsync(agent, systemMessage, thread);

const string exitPrompt = "exit";
Console.WriteLine($"(Interactive chat started. Type '{exitPrompt}' to quit.)\n");

for (; ;) {
    var (isValid, userMessage) = GetUserMessage();
    if (!isValid)
        break;
    await RunAsync(agent, userMessage, thread);
}

foreach (var mcpClient in mcpClients)
    await mcpClient.DisposeAsync();

static async Task RunAsync(AIAgent agent, ChatMessage chatMessage, AgentThread? thread = null)
{
    try {
        var response = await agent.RunAsync(chatMessage, thread);
        Console.WriteLine($"Agent: {response.Text ?? string.Empty}\n");
    } catch (Exception ex) {
        Console.WriteLine($"Error running agent: {ex.Message}");
    }
}

static ChatMessage GetSystemMessage()
{
    return new(ChatRole.System, GetSystemPrompt());

    static string GetSystemPrompt()
    {
        string systemPrompt;
        try {
            var instructionPath = Path.Combine(AppContext.BaseDirectory, systemPromptFileName);
            systemPrompt = File.ReadAllText(instructionPath);
        } catch {
            systemPrompt = string.Empty;
        }
        return systemPrompt;
    }
}

static (bool isValid, ChatMessage userMessage) GetUserMessage()
{
    var (isValid, userPrompt) = GetUserPrompt();
    return (isValid, new(ChatRole.User, userPrompt));

    static (bool isValid, string userPrompt) GetUserPrompt()
    {
        Console.Write("You: ");
        var userPrompt = Console.ReadLine();
        Console.WriteLine();

        return string.IsNullOrWhiteSpace(userPrompt) ||
               string.Equals(userPrompt.Trim(), exitPrompt, StringComparison.OrdinalIgnoreCase)
            ? (isValid: false, userPrompt: string.Empty)
            : (isValid: true , userPrompt: userPrompt! );
    }
}

static IChatClient GetChatClient(ChatClientType chatClientType)
    => chatClientType switch {
    ChatClientType.GitHub      => GetGitHubClient     (),
    ChatClientType.AzureOpenAI => GetAzureOpenAIClient(),
    ChatClientType.Ollama      => GetOllamaClient     (),
    _ => throw new NotSupportedException($"Chat client type '{chatClientType}' is not supported.")
};

static IChatClient GetGitHubClient()
{
    var gitHubToken = GetGitHubToken();

    var chatClient =
        new ChatClient(
            model: "gpt-4o-mini",
            credential: new ApiKeyCredential(gitHubToken!),
            options: new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") }
        ).AsIChatClient()
         .AsBuilder()
         .UseFunctionInvocation()
         .Build();
    return chatClient;
}

static string GetGitHubToken()
{
    const string GitHubEnvironmentVariable = "GITHUB_TOKEN";
    var gitHubToken = Environment.GetEnvironmentVariable(GitHubEnvironmentVariable);
    if (string.IsNullOrEmpty(gitHubToken))
        throw new InvalidOperationException($"Please set the {GitHubEnvironmentVariable} environment variable.");
    return gitHubToken!;
}

static IChatClient GetAzureOpenAIClient()
{
    var azureOpenAIEndPoint = GetEndPoint();
    var openAIApiKey        = GetKey     ();

    var credential = new AzureKeyCredential(openAIApiKey!);
    const string deploymentName = "202406AzureAIModel";
    var azureOpenAIClient = new AzureOpenAIClient(new Uri(azureOpenAIEndPoint!), credential);
    var chatClient = azureOpenAIClient.GetChatClient(deploymentName)
                                      .AsIChatClient();
    return chatClient;

    static string GetEndPoint()
    {
        const string AzureOpenAIEndpointEnvironmentVariable = "AZURE_OPENAI_ENDPOINT";
        var azureOpenAIEndPoint = Environment.GetEnvironmentVariable(AzureOpenAIEndpointEnvironmentVariable);
        if (string.IsNullOrEmpty(azureOpenAIEndPoint))
            throw new InvalidOperationException($"Please set the {AzureOpenAIEndpointEnvironmentVariable} environment variable.");
        return azureOpenAIEndPoint;
    }

    static string GetKey()
    {
        const string AzureOpenAIApiKeyEnvironmentVariable = "AZURE_OPENAI_API_KEY";
        var openAIApiKey = Environment.GetEnvironmentVariable(AzureOpenAIApiKeyEnvironmentVariable);
        if (string.IsNullOrEmpty(openAIApiKey))
            throw new InvalidOperationException($"Please set the {AzureOpenAIApiKeyEnvironmentVariable} environment variable.");
        return openAIApiKey!;
    }
}

static IChatClient GetOllamaClient()
{
    var uri = new Uri("http://localhost:11434");
    var ollama = new OllamaApiClient(uri);
    ollama.SelectedModel = "gpt-oss:20b-cloud";

    IChatClient chatClient = ollama;
    chatClient = chatClient.AsBuilder()
                           .UseFunctionInvocation() // ツール呼び出しを使う
                           .Build();
    return chatClient;
}

static async Task<(McpClient, IList<McpClientTool>)> GetMcpServerTools(McpServer mcpServer)
{
    IClientTransport clientTransport = GetMcpClientTransport(mcpServer);
    McpClient client = await McpClient.CreateAsync(clientTransport);

    IList<McpClientTool> tools = await client.ListToolsAsync();
    foreach (var tool in tools)
        Debug.WriteLine($"{tool.Name} ({tool.Description})");
    return (client, tools);
}

static IClientTransport GetMcpClientTransport(McpServer mcpServer)
{
    var clientTransport = mcpServer switch {
        McpServer.Weather    => GetWeatherToolClientTransport   (),
        McpServer.Time       => GetTimeToolClientTransport      (),
        McpServer.FileSystem => GetFileSystemToolClientTransport(),
        McpServer.GitHub     => GetGitHubToolClientTransport    (),
        _ => throw new NotSupportedException($"MCP server '{mcpServer}' is not supported.")
    };
    Debug.WriteLine(clientTransport.Name);
    return clientTransport;

    static IClientTransport GetTimeToolClientTransport()
        => new StdioClientTransport(new() {
            Name      = "time"  ,
            Command   = "dotnet",
            Arguments = ["run", "--project", @"C:\DropBox\Dropbox\Source\GitHub\Repos\2025.10.AIAgentsSeminarSlide\Shos.AIAgentSample\MCPServer.Console\MCPServer.Console.csproj"]
        });

    static IClientTransport GetWeatherToolClientTransport()
    {
        const string endPoint = "https://localhost:7052/sse";
        return new HttpClientTransport(new HttpClientTransportOptions { Endpoint = new Uri(endPoint) });
    }

    static IClientTransport GetFileSystemToolClientTransport()
        => new StdioClientTransport(new() {
            Name      = "filesystem",
            Command   = "npx",
            Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "/work"]
        });

    static IClientTransport GetGitHubToolClientTransport()
        => new StdioClientTransport(new() {
            Name      = "github",
            Command   = "npx",
            Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-github"],
            EnvironmentVariables = new Dictionary<string, string?> { ["GITHUB_PERSONAL_ACCESS_TOKEN"] = GetGitHubToken() }
        });
}

enum ChatClientType
{
    GitHub     ,
    AzureOpenAI,
    Ollama
}

enum McpServer
{
    Time      ,
    Weather   ,
    FileSystem,
    GitHub
}
