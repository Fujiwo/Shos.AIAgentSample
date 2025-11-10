using System.ComponentModel;

namespace FCAICad
{
    internal static class Program
    {
        static MainForm? mainForm;
        static MyChatAgent chatAgent = new();

        /// <summary> The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            mainForm = new MainForm();
            mainForm.Prompted += OnPrompted;

            Application.Run(mainForm);
        }

        static async void OnPrompted(object? sender, string prompt)
        {
            var response = await chatAgent.GetResponseAsync(prompt);
            if (mainForm is not null)
                mainForm.AppendResponse($"Agent: {response}{Environment.NewLine}");
            mainForm.OnPromptedEnd();
        }

        [Description("Get the paper size (width and height).")]
        public static (double width, double height) GetPaperSize()
        {
            var paperSize = mainForm?.PaperSize ?? Vector2D.Zero;
            return (paperSize.X, paperSize.Y);
        }

        [Description("Draw a line.")]
        public static void DrawLine(
            [Description("The x coordinate of starting point of the line.")]
            double startX,
            [Description("The y coordinate of starting point of the line.")]
            double startY,
            [Description("The x coordinate of ending point of the line.")]
            double endX,
            [Description("The y coordinate of ending point of the line.")]
            double endY)
            => mainForm?.AddFigure(new LineFigure { Start = new Vector2D { X = startX, Y = startY },
                                                    End   = new Vector2D { X = endX  , Y = endY   } });
    }
}