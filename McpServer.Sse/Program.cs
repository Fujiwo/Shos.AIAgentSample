// Program.cs
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();
var application = builder.Build();

application.MapMcp();
application.Run("http://localhost:3001");

[McpServerToolType, Description("天気を予報する")]
public static class WeatherForecastTool
{
    [McpServerTool, Description("指定した場所の天気を予報します")]
    public static string GetWeatherForecast(
        [Description("天気を予報したい都道府県名")]
        string location) => location switch {
            "北海道" => "晴れ",
            "東京都" => "曇り",
            "石川県" => "雨",
            "福井県" => "雪",
            _       => "晴か曇りか雨か雪か霙か霰か何か"
        };
}
