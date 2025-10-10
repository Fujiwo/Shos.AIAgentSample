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