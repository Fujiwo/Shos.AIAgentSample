using MCPServerLab;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer().WithTools<WeatherForecastTool>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/", () => "My MCP server.");
app.MapMcpSse();
app.Run();

[McpServerToolType, Description("ìVãCÇó\ïÒÇ∑ÇÈ")]
class WeatherForecastTool
{
    [McpServerTool, Description("éwíËÇµÇΩèÍèäÇÃìVãCÇó\ïÒÇµÇ‹Ç∑")]
    public static string GetWeatherForecast(
        [Description("ìVãCÇó\ïÒÇµÇΩÇ¢ìsìπï{åßñº")]
        string location) => location switch {
            "ñkäCìπ" => "ê∞ÇÍ",
            "ìåãûìs" => "ì‹ÇË",
            "êŒêÏåß" => "âJ"  ,
            "ïüà‰åß" => "ê·"  ,
            _       => "ê∞Ç©ì‹ÇËÇ©âJÇ©ê·Ç©Ë¬Ç©Ë≈Ç©âΩÇ©"
        };
}