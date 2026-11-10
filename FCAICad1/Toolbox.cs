using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace FCAICad;

// AI エージェントに「ツール」として公開する CAD 操作の関数群
// - public static メソッドは ChatAgent.cs の GetCadTools で AIFunctionFactory.Create により AI 関数ツールに変換される
// - [Description] 属性の文字列は、ツールや引数の説明として AI に渡される (AI はこれを読んで、どのツールをどの引数で呼ぶかを判断する)
// - 座標は用紙 (作図領域) の左上が原点で、右方向が X+、下方向が Y+。長さ・座標の単位は GetPaperSize で返す用紙サイズと同じ
// - ツールは UI スレッド以外から呼ばれうるため、画面 (モデル) の操作は MainForm のメソッド経由で UI スレッド上で行う
static class Toolbox
{
    // 描画先のメイン フォーム (起動時に Program.Run で設定される)
    public static MainForm? MainForm { get; set; }

    // 用紙 (作図領域) のサイズ (幅と高さ) を返す。フォームが未設定なら空のサイズ
    [Description("Get the paper size (width and height).")]
    public static SizeF GetPaperSize() => MainForm?.PaperSize ?? SizeF.Empty;

    // 描かれた図形をすべて消去し、結果を文字列で返す
    [Description("Clear all drawn shapes to make the paper blank.")]
    public static string ClearAll()
    {
        MainForm?.Clear();
        return "Cleared.";
    }

    // 色名を検証してから図形を追加し、結果を文字列で返す (結果は AI エージェントに伝わる)
    static string AddFigure(string color, Func<Figure> createFigure)
    {
        if (!availableColors.Contains(color, StringComparer.OrdinalIgnoreCase))
            return $"Error: '{color}' is not an available color. Use one of the colors from GetAvailableColors.";
        if (MainForm is null)
            return "Error: The drawing area is not ready.";

        var figure = createFigure();
        // MainForm.AddFigure は UI スレッド上でモデルに図形を追加する (追加されるとビューに描画される)
        MainForm.AddFigure(figure);
        return $"Drawn: {figure}";
    }

    // 使用できる色名の一覧を返す (配列のコピーを返し、元の一覧が書き換えられないようにする)
    [Description("Get the available colors for drawing.")]
    public static string[] GetAvailableColors() => [.. availableColors];

    // 使用できる色名 (System.Drawing.Color.FromName で色に変換できる名前)
    static readonly string[] availableColors = [
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

    // 以下は図形を描くツール。共通の引数と戻り値:
    // - color: 色名 (GetAvailableColors で返す一覧のいずれか。それ以外はエラーを返す)
    // - lineWidth: 線幅 (3〜30 の範囲は AI への説明で指示しているだけで、コードでは検証しない)
    // - isFilled: true なら塗りつぶす
    // - 戻り値: 描いた図形の内容 ("Drawn: ...") またはエラー ("Error: ...")。AI はこれを見て次の操作を決める

    // 直線を描く (start から end まで)
    [Description("Draw a line.")]
    public static string DrawLine(
        [Description("The color of the line.")]
            string color,
        [Description("The line width (3 to 30) of the line.")]
            float lineWidth,
        [Description("The starting point of the line.")]
            PointF start,
        [Description("The ending point of the line.")]
            PointF end
    )
        => AddFigure(color, () => new LineFigure { ColorName = color, LineWidth = lineWidth, Start = start, End = end });

    // 矩形を描く (shape は左上の座標と幅・高さ)
    [Description("Draw a rectangle.")]
    public static string DrawRectangle(
        [Description("The color of the rectangle.")]
            string color,
        [Description("The line width (3 to 30) of the rectangle.")]
            float lineWidth,
        [Description("The shape of rectangle.")]
            RectangleF shape,
        [Description("The rectangle is filled or not.")]
            bool isFilled
    )
        => AddFigure(color, () => new RectangleFigure { ColorName = color, LineWidth = lineWidth, Shape = shape, IsFilled = isFilled });

    // 角丸矩形を描く (radius は角の丸みの幅方向・高さ方向の半径)
    [Description("Draw a rounded rectangle.")]
    public static string DrawRoundedRectangle(
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
        => AddFigure(color, () => new RoundedRectangleFigure { ColorName = color, LineWidth = lineWidth, Shape = shape, Radius = radius, IsFilled = isFilled });

    // 円を描く (中心と半径で指定)
    [Description("Draw a circle.")]
    public static string DrawCircle(
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
        => AddFigure(color, () => new CircleFigure { ColorName = color, LineWidth = lineWidth, Center = center, Radius = radius, IsFilled = isFilled });

    // 楕円を描く (中心と X 方向・Y 方向の半径で指定)
    [Description("Draw a ellipse.")]
    public static string DrawEllipse(
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
        => AddFigure(color, () => new EllipseFigure { ColorName = color, LineWidth = lineWidth, Center = center, RadiusX = radiusX, RadiusY = radiusY, IsFilled = isFilled });

    // 点列を直線で結ぶ。isClosed が true なら多角形、false なら折れ線 (塗りつぶしは閉じた場合のみ)
    [Description("Draw a polyline or polygon with points.")]
    public static string DrawPolylineOrPolygon(
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
        => AddFigure(color, () => new PolylineFigure { ColorName = color, LineWidth = lineWidth, Points = points, IsClosed = isClosed, IsFilled = isFilled });

    // 点列を通る滑らかな曲線 (カーディナル スプライン) を描く。isClosed が true なら閉曲線 (塗りつぶしは閉じた場合のみ)
    [Description("Draw a curve with points.")]
    public static string DrawCurve(
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
        => AddFigure(color, () => new CurveFigure { ColorName = color, LineWidth = lineWidth, Points = points, IsClosed = isClosed, IsFilled = isFilled });

    // 点列をベジエ曲線でつないだ自由曲線を描く (前の点に近すぎる点は間引かれる。塗りつぶしは無し)
    [Description("Draw a free-form curve with points.")]
    public static string DrawFreeFormCurve(
        [Description("The color of the free-form curve.")]
            string color,
        [Description("The line width (3 to 30) of the free-form curve.")]
            float lineWidth,
        [Description("Points that make up the free-form curve.")]
            PointF[] points,
        [Description("The curve is closed or not.")]
            bool isClosed
    )
        => AddFigure(color, () => new FreeFormCurveFigure { ColorName = color, LineWidth = lineWidth, Points = points, IsClosed = isClosed });
}
