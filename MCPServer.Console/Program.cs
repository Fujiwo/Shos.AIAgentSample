using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class TimeTools
{
    [McpServerTool, Description("現在の時刻を取得")]
    public static string GetCurrentTime() => DateTimeOffset.Now.ToString();

    [McpServerTool, Description("指定されたタイムゾーンの現在の時刻を取得")]
    public static string GetTimeInTimezone(string timezone)
    {
        try {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone).ToString();
        } catch {
            return "無効なタイムゾーンが指定されています";
        }
    }
}

/*
npx @modelcontextprotocol/inspector dotnet run --project ./MCPServer.Console/MCPServer.Console.csproj
http://127.0.0.1:6274

Visual Studio Code - 設定 - MCP - settings.json で編集

"servers": {
    "MCPServer.Console": {
        "type": "stdio",
        "command": "dotnet",
        "args": [
            "run",
            "--project",
            "C:\\[プロジェクト フォルダー]\\MCPServer.Console.csproj"
        ]
    }
}
 */
