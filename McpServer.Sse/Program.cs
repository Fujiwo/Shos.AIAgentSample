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

[McpServerToolType, Description("�V�C��\�񂷂�")]
public static class WeatherForecastTool
{
    [McpServerTool, Description("�w�肵���ꏊ�̓V�C��\�񂵂܂�")]
    public static string GetWeatherForecast(
        [Description("�V�C��\�񂵂����s���{����")]
        string location) => location switch {
            "�k�C��" => "����",
            "�����s" => "�܂�",
            "�ΐ쌧" => "�J",
            "���䌧" => "��",
            _       => "�����܂肩�J���Ⴉ���ł�����"
        };
}
