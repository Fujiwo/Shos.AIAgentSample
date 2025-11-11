using System.ComponentModel;

namespace FCAICad
{
    internal static class Program
    {
        static MainForm? mainForm;
        static MyChatAgent chatAgent = new();

        /// <summary> The main entry point for the application.</summary>
        [STAThread]
        static async Task Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            mainForm = new MainForm();
            mainForm.Prompted += OnPrompted;

            Application.Run(mainForm);
            await chatAgent.DisposeAsync();
        }

        static async void OnPrompted(object? sender, string prompt)
        {
            var response = await chatAgent.GetResponseAsync(prompt);
            if (mainForm is not null) {
                mainForm.AppendResponse($"Agent: {response}{Environment.NewLine}");
                mainForm.OnPromptedEnd();
            }
        }

        [Description("Get the paper size (width and height).")]
        public static SizeF GetPaperSize() => mainForm?.PaperSize ?? new SizeF();

        [Description("Clear all drawn shapes to make the paper blank.")]
        public static void ClearAll() => mainForm?.Clear();

        [Description("Get the available colors for drawing.")]
        public static string[] GetAvailableColors() => [
            "MediumAquamarine",
            "MediumBlue",
            "MediumOrchid",
            "MediumPurple",
            "MediumSeaGreen",
            "MediumSlateBlue",
            "MediumSpringGreen",
            "MediumTurquoise",
            "MediumVioletRed",
            "MidnightBlue",
            "MintCream",
            "MistyRose",
            "Moccasin",
            "NavajoWhite",
            "Navy",
            "OldLace",
            "Olive",
            "Maroon",
            "OliveDrab",
            "Magenta",
            "LimeGreen",
            "LavenderBlush",
            "LawnGreen",
            "LemonChiffon",
            "LightBlue",
            "LightCoral",
            "LightCyan",
            "LightGoldenrodYellow",
            "LightGray",
            "LightGreen",
            "LightPink",
            "LightSalmon",
            "LightSeaGreen",
            "LightSkyBlue",
            "LightSlateGray",
            "LightSteelBlue",
            "LightYellow",
            "Lime",
            "Linen",
            "Yellow",
            "Orange",
            "Orchid",
            "Silver",
            "SkyBlue",
            "SlateBlue",
            "SlateGray",
            "Snow",
            "SpringGreen",
            "SteelBlue",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato",
            "Transparent",
            "Turquoise",
            "Violet",
            "Wheat",
            "White",
            "WhiteSmoke",
            "Sienna",
            "OrangeRed",
            "SeaShell",
            "SandyBrown",
            "PaleGoldenrod",
            "PaleGreen",
            "PaleTurquoise",
            "PaleVioletRed",
            "PapayaWhip",
            "PeachPuff",
            "Peru",
            "Pink",
            "Plum",
            "PowderBlue",
            "Purple",
            "RebeccaPurple",
            "Red",
            "RosyBrown",
            "RoyalBlue",
            "SaddleBrown",
            "Salmon",
            "SeaGreen",
            "Khaki",
            "Lavender",
            "Cyan",
            "DarkMagenta",
            "DarkKhaki",
            "DarkGreen",
            "DarkGray",
            "DarkGoldenrod",
            "DarkCyan",
            "DarkBlue",
            "Ivory",
            "Crimson",
            "Cornsilk",
            "CornflowerBlue",
            "Coral",
            "Chocolate",
            "DarkOliveGreen",
            "Chartreuse",
            "BurlyWood",
            "Brown",
            "BlueViolet",
            "Blue",
            "BlanchedAlmond",
            "Black",
            "Bisque",
            "Beige",
            "Azure",
            "Aquamarine",
            "Aqua",
            "AntiqueWhite",
            "AliceBlue",
            "CadetBlue",
            "DarkOrange",
            "YellowGreen",
            "DarkRed",
            "Indigo",
            "IndianRed",
            "DarkOrchid",
            "Honeydew",
            "GreenYellow",
            "Green",
            "Gray",
            "Goldenrod",
            "Gold",
            "GhostWhite",
            "Gainsboro",
            "Fuchsia",
            "ForestGreen",
            "HotPink",
            "Firebrick",
            "FloralWhite",
            "DodgerBlue",
            "DimGray",
            "DeepSkyBlue",
            "DeepPink",
            "DarkViolet",
            "DarkTurquoise",
            "DarkSlateGray",
            "DarkSlateBlue",
            "DarkSeaGreen",
            "DarkSalmon"
        ];

        [Description("Draw a line.")]
        public static void DrawLine(
            [Description("The color of the line.")]
            string color,
            [Description("The starting point of the line.")]
            PointF start,
            [Description("The ending point of the line.")]
            PointF end
        )
            => mainForm?.AddFigure(new LineFigure { Color = Color.FromName(color), Start = start, End = end });

        [Description("Draw a circle.")]
        public static void DrawCircle(
            [Description("The color of the circle.")]
            string color,
            [Description("The center point of the circle.")]
            PointF center,
            [Description("The radius of the circle.")]
            float radius
        )
            => mainForm?.AddFigure(new CircleFigure { Color = Color.FromName(color), Center = center, Radius = radius });

        [Description("Draw a ellipse.")]
        public static void DrawEllipse(
            [Description("The color of the ellipse.")]
            string color,
            [Description("The center point of the ellipse.")]
            PointF center,
            [Description("The radius of the ellipse in the x-direction.")]
            float radiusX,
            [Description("The radius of the ellipse in the y-direction.")]
            float radiusY
        )
            => mainForm?.AddFigure(new EllipseFigure { Color = Color.FromName(color), Center = center, RadiusX = radiusX, RadiusY = radiusY });

        [Description("Draw a free-form curve with points.")]
        public static void DrawFreeLine(
            [Description("The color of the free line.")]
            string color,
            [Description("Points that make up the free curve.")]
            PointF[] points
        )
            => mainForm?.AddFigure(new FreeLineFigure { Color = Color.FromName(color), Points = points });
    }
}