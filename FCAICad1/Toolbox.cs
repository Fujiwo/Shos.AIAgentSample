using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace FCAICad;

static class Toolbox
{
    public static MainForm? MainForm { get; set; }

    [Description("Get the paper size (width and height).")]
    public static SizeF GetPaperSize() => MainForm?.PaperSize ?? new SizeF();

    [Description("Clear all drawn shapes to make the paper blank.")]
    public static void ClearAll() => MainForm?.Clear();

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
        [Description("The line width (3 to 30) of the line.")]
            float lineWidth,
        [Description("The starting point of the line.")]
            PointF start,
        [Description("The ending point of the line.")]
            PointF end
    )
        => MainForm?.AddFigure(new LineFigure { ColorName = color, LineWidth = lineWidth, Start = start, End = end });

    [Description("Draw a rectangle.")]
    public static void DrawRectangle(
        [Description("The color of the rectangle.")]
            string color,
        [Description("The line width (3 to 30) of the rectangle.")]
            float lineWidth,
        [Description("The shape of rectangle.")]
            RectangleF shape,
        [Description("The rectangle is filled or not.")]
            bool isFilled
    )
        => MainForm?.AddFigure(new RectangleFigure { ColorName = color, LineWidth = lineWidth, Shape = shape, IsFilled = isFilled });

    [Description("Draw a rounded rectangle.")]
    public static void DrawRoundedRectangle(
        [Description("The color of the rounded rectangle.")]
            string color,
        [Description("The line width (3 to 30) of the rounded rectangle.")]
            float lineWidth,
        [Description("The shape of rounded rectangle.")]
            RectangleF shape,
        [Description("The radius of rounded rectangle.")]
            SizeF radius,
        [Description("The rectangle is filled or not.")]
            bool isFilled
    )
        => MainForm?.AddFigure(new RoundedRectangleFigure { ColorName = color, LineWidth = lineWidth, Shape = shape, Radius = radius, IsFilled = isFilled });

    [Description("Draw a circle.")]
    public static void DrawCircle(
        [Description("The color of the circle.")]
            string color,
        [Description("The line width (3 to 30) of the circle.")]
            float lineWidth,
        [Description("The center point of the circle.")]
            PointF center,
        [Description("The radius of the circle.")]
            float radius,
        [Description("The circle is filled or not.")]
            bool isFilled
    )
        => MainForm?.AddFigure(new CircleFigure { ColorName = color, LineWidth = lineWidth, Center = center, Radius = radius, IsFilled = isFilled });

    [Description("Draw a ellipse.")]
    public static void DrawEllipse(
        [Description("The color of the ellipse.")]
            string color,
        [Description("The line width (3 to 30) of the ellipse.")]
            float lineWidth,
        [Description("The center point of the ellipse.")]
            PointF center,
        [Description("The radius of the ellipse in the x-direction.")]
            float radiusX,
        [Description("The radius of the ellipse in the y-direction.")]
            float radiusY,
        [Description("The ellipse is filled or not.")]
            bool isFilled
    )
        => MainForm?.AddFigure(new EllipseFigure { ColorName = color, LineWidth = lineWidth, Center = center, RadiusX = radiusX, RadiusY = radiusY, IsFilled = isFilled });

    [Description("Draw a polyline or polygon with points.")]
    public static void DrawPolylineOrPolygon(
        [Description("The color of the polyline or polygon.")]
            string color,
        [Description("The line width (3 to 30) of the polyline or polygon.")]
            float lineWidth,
        [Description("Points that make up the polyline or polygon.")]
            PointF[] points,
        [Description("The polyline or polygon is closed or not.")]
            bool isClosed,
        [Description("The polyline or polygon is filled or not.")]
            bool isFilled
    )
        => MainForm?.AddFigure(new PolylineFigure { ColorName = color, LineWidth = lineWidth, Points = points, IsClosed = isClosed, IsFilled = isFilled });

    [Description("Draw a curve with points.")]
    public static void DrawCurve(
        [Description("The color of the curve.")]
            string color,
        [Description("The line width (3 to 30) of the curve.")]
            float lineWidth,
        [Description("Points that make up the curve.")]
            PointF[] points,
        [Description("The curve is closed or not.")]
            bool isClosed,
        [Description("The curve is filled or not.")]
            bool isFilled
    )
        => MainForm?.AddFigure(new CurveFigure { ColorName = color, LineWidth = lineWidth, Points = points, IsClosed = isClosed, IsFilled = isFilled });

    [Description("Draw a free-form curve with points.")]
    public static void DrawFreeFormCurve(
        [Description("The color of the free-form curve.")]
            string color,
        [Description("The line width (3 to 30) of the free-form curve.")]
            float lineWidth,
        [Description("Points that make up the free-form curve.")]
            PointF[] points,
        [Description("The curve is closed or not.")]
            bool isClosed
    )
        => MainForm?.AddFigure(new FreeFormCurveFigure { ColorName = color, LineWidth = lineWidth, Points = points, IsClosed = isClosed });
}
