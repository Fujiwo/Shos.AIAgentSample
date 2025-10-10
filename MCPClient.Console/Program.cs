using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;

//await RunAsync(McpServer.Time);
//await RunAsync(McpServer.Weather, new Dictionary<string, object?> { ["location"] = "福井県" });
await RunAsync(McpServer.FileSystem);
//await RunAsync(McpServer.GitHub);

var (mcpClient1, tools1) = await GetMcpServerTools(McpServer.Time      );
var (mcpClient2, tools2) = await GetMcpServerTools(McpServer.Weather   );
var (mcpClient3, tools3) = await GetMcpServerTools(McpServer.FileSystem);
var (mcpClient4, tools4) = await GetMcpServerTools(McpServer.GitHub    );
IEnumerable<McpClientTool> tools = [.. tools1, .. tools2, .. tools3, .. tools4];
McpClient[] mcpClients = [mcpClient1, mcpClient2, mcpClient3, mcpClient4];

var chatClient = GetAzureOpenAIClient();

var client = chatClient.AsIChatClient()
                       .AsBuilder()
                       .UseFunctionInvocation()
                       .Build();

const string agentName = "AIエージェント";
const string instructions = "あなたはAIエージェントです。";

AIAgent agent = new ChatClientAgent(
    client,
    new ChatClientAgentOptions {
        Name = agentName,
        Instructions = instructions,
        ChatOptions = new ChatOptions { Tools = tools.Cast<AITool>().ToList() }
    }
);

var result = await agent.RunAsync("福井県の天気を教えて?");
Console.WriteLine(result.Text);

foreach (var mcpClient in mcpClients)
    await mcpClient.DisposeAsync();

static IClientTransport GetMcpClientTransport(McpServer mcpServer)
{
    var clientTransport = mcpServer switch {
        McpServer.Weather    => GetWeatherToolClientTransport   (),
        McpServer.Time       => GetTimeToolClientTransport      (),
        McpServer.FileSystem => GetFileSystemToolClientTransport(),
        McpServer.GitHub     => GetGitHubToolClientTransport    (),
        _ => throw new NotSupportedException($"MCP server '{mcpServer}' is not supported.")
    };
    Console.WriteLine(clientTransport.Name);
    return clientTransport;

    static IClientTransport GetWeatherToolClientTransport()
    {
        const string endPoint = "https://localhost:7052/sse";
        return new HttpClientTransport(new HttpClientTransportOptions { Endpoint = new Uri(endPoint) });
    }

    static IClientTransport GetTimeToolClientTransport()
        => new StdioClientTransport(new() {
            Name      = "time"  ,
            Command   = "dotnet",
            Arguments = ["run", "--project", @"C:\DropBox\Dropbox\Source\GitHub\Repos\2025.10.AIAgentsSeminarSlide\Shos.AIAgentSample\MCPServer.Console\MCPServer.Console.csproj"]
        });


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

static async Task RunAsync(McpServer mcpServer, IReadOnlyDictionary<string, object?>? arguments = null)
{
    IClientTransport clientTransport = GetMcpClientTransport(mcpServer);

    await using var client = await McpClient.CreateAsync(clientTransport);
    IList<McpClientTool> tools = await client.ListToolsAsync();
    foreach (var tool in tools) {
        Console.WriteLine($"{tool.Name} ({tool.Description})");
        try {
            var response = await client.CallToolAsync(tool.Name, arguments);
            foreach (var contentBlock in response.Content)
                Console.WriteLine($"ContentBlock: {(contentBlock as TextContentBlock)?.Text}");
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

static async Task<(McpClient, IList<McpClientTool>)> GetMcpServerTools(McpServer mcpServer)
{
    IClientTransport clientTransport = GetMcpClientTransport(mcpServer);
    McpClient client = await McpClient.CreateAsync(clientTransport);

    IList<McpClientTool> tools = await client.ListToolsAsync();
    foreach (var tool in tools)
        Console.WriteLine($"{tool.Name} ({tool.Description})");
    return (client, tools);
}

static string GetGitHubToken()
{
    const string GitHubEnvironmentVariable = "GITHUB_TOKEN";
    var gitHubToken = Environment.GetEnvironmentVariable(GitHubEnvironmentVariable);
    if (string.IsNullOrEmpty(gitHubToken))
        throw new InvalidOperationException($"Please set the {GitHubEnvironmentVariable} environment variable.");
    return gitHubToken!;
}

static ChatClient GetAzureOpenAIClient()
{
    var azureOpenAIEndPoint = GetEndPoint();
    var openAIApiKey = GetKey();

    var credential = new AzureKeyCredential(openAIApiKey!);
    const string deploymentName = "202406AzureAIModel";
    var azureOpenAIClient = new AzureOpenAIClient(new Uri(azureOpenAIEndPoint!), credential);
    var chatClient = azureOpenAIClient.GetChatClient(deploymentName);
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

enum McpServer
{
    Time      ,
    Weather   ,
    FileSystem,
    GitHub
}

//await Run(client);

//Console.ReadKey();
//// */

//async Task Run(IMcpClient client, IReadOnlyDictionary<string, object?>? arguments = null) {
//    // ツールの一覧からツールを取得
//    foreach (var tool in await client.ListToolsAsync()) {
//        Console.WriteLine($"{tool.Name} ({tool.Description})");
//        // ツールを実行
//        var response = await client.CallToolAsync(tool.Name, arguments);
//        // レスポンスを表示
//        foreach (var content in response.Content)
//            Console.WriteLine($"tool.Name: {tool.Name}, content.Type: {content.Type}, content.Text: {content..Text}");
//    }
//}
