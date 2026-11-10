using System.Collections;

namespace FCAICad;

public class Model : IEnumerable<Figure>
{
    public SizeF Size { get; } = new(width: 3000.0f, height: 2000.0f);

    public event EventHandler<Figure?>? Update;

    readonly List<Figure> figures = [];

    public RectangleF Bounds {
        get {
            if (figures.Count <= 0)
                return RectangleF.Empty;
            var bounds = figures[0].Bounds;
            for (var index = 1; index < figures.Count; index++)
                bounds = RectangleF.Union(bounds, figures[index].Bounds);
            return bounds;
        }
    }

    public void Add(Figure figure)
    {
        figures.Add(figure);
        Update?.Invoke(this, figure);
        figure.Log();
    }

    public void Clear()
    {
        figures.Clear();
        Update?.Invoke(this, null);
    }

    public IEnumerator<Figure> GetEnumerator() => figures.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public abstract class Figure
{
    public Color Color { get; set; } = Color.Black;
    public string ColorName { set => Color = Color.FromName(value); }
    protected Brush Brush => new SolidBrush(Color);

    public float LineWidth { get; set; } = 10.0f;

    public RectangleF Bounds
    {
        get {
            var bounds = ShapeBounds;
            bounds.Inflate(LineWidth / 2.0f, LineWidth / 2.0f);
            return bounds;
        }
    }

    public abstract RectangleF ShapeBounds { get; }

    public void Draw(Graphics graphics)
    {
        using Pen pen = new(Color, LineWidth);
        DrawShape(graphics, pen);
    }

    public override string ToString()
        => $"{GetType().Name} (Color: {Color.Name}, LineWidth: {LineWidth})";

    protected virtual void DrawShape(Graphics graphics, Pen pen) { }
}

public class LineFigure : Figure
{
    public required PointF Start { get; init; }
    public required PointF End   { get; init; }

    public override RectangleF ShapeBounds => new RectangleF(x     : Math.Min(Start.X, End.X ),
                                                             y     : Math.Min(Start.Y, End.Y ),
                                                             width : Math.Abs(End.X - Start.X),
                                                             height: Math.Abs(End.Y - Start.Y));

    public override string ToString()
        => $"{base.ToString()}, Start: {Start}, End: {End}";

    protected override void DrawShape(Graphics graphics, Pen pen)
        =>  graphics.DrawLine(pen, Start, End);
}

public class RectangleFigure : Figure
{
    public required RectangleF Shape { get; init; }
    public bool IsFilled { get; set; } = true;

    public override RectangleF ShapeBounds => Shape;

    public override string ToString()
        => $"{base.ToString()}, Shape: {Shape}";

    protected override void DrawShape(Graphics graphics, Pen pen)
    {
        if (IsFilled) {
            using Brush brush = Brush;
            graphics.FillRectangle(brush, Shape);
        }
        graphics.DrawRectangle(pen, Shape);
    }
}

public class RoundedRectangleFigure : RectangleFigure
{
    public SizeF Radius { get; set; } = new SizeF(width: 1.0f, height: 1.0f);

    protected override void DrawShape(Graphics graphics, Pen pen)
    {
        if (IsFilled) {
            using Brush brush = Brush;
            graphics.FillRoundedRectangle(brush, Shape, Radius);
        }
        graphics.DrawRoundedRectangle(pen, Shape, Radius);
    }
}

public class CircleFigure : Figure
{
    public required PointF Center { get; init; }
    public required float Radius { get; init; }
    public bool IsFilled { get; set; } = true;

    public override RectangleF ShapeBounds => new RectangleF(x     : Center.X - Radius,
                                                             y     : Center.Y - Radius,
                                                             width : Radius + Radius  ,
                                                             height: Radius + Radius  );

    public override string ToString()
        => $"{base.ToString()}, Center: {Center}, Radius: {Radius}";

    protected override void DrawShape(Graphics graphics, Pen pen)
    {
        if (IsFilled) {
            using Brush brush = Brush;
            graphics.FillEllipse(brush, Center.X - Radius, Center.Y - Radius, Radius + Radius, Radius + Radius);
        }
        graphics.DrawEllipse(pen, Center.X - Radius, Center.Y - Radius, Radius + Radius, Radius + Radius);
   }
}

public class EllipseFigure : Figure
{
    public required PointF Center { get; init; }
    public required float RadiusX { get; init; }
    public required float RadiusY { get; init; }

    public bool IsFilled { get; set; } = true;

    public override RectangleF ShapeBounds => new RectangleF(x     : Center.X - RadiusX,
                                                             y     : Center.Y - RadiusY,
                                                             width : RadiusX + RadiusX ,
                                                             height: RadiusY + RadiusY );

    public override string ToString()
        => $"{base.ToString()}, Center: {Center}, RadiusX: {RadiusX}, RadiusY: {RadiusY}";

    protected override void DrawShape(Graphics graphics, Pen pen)
    {
        if (IsFilled) {
            using Brush brush = Brush;
            graphics.FillEllipse(brush, Center.X - RadiusX, Center.Y - RadiusY, RadiusX + RadiusX, RadiusY + RadiusY);
        }
        graphics.DrawEllipse(pen, Center.X - RadiusX, Center.Y - RadiusY, RadiusX + RadiusX, RadiusY + RadiusY);
    }
}

public abstract class PointsFigure : Figure
{
    // C# 14 の field キーワードで、自動生成されるバッキングフィールドを参照する
    public IList<PointF> Points
    {
        get;
        set {
            field.Clear();
            value.ForEach(point => Add(point));
        }
    } = [];

    public bool IsClosed { get; set; } = false;

    public override RectangleF ShapeBounds
    {
        get {
            if (Points.Count <= 0)
                return RectangleF.Empty;
            var minX = Points[0].X;
            var minY = Points[0].Y;
            var maxX = Points[0].X;
            var maxY = Points[0].Y;
            Points.ForEach(point => {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            });
            return new RectangleF(x     : minX       ,
                                  y     : minY       ,
                                  width : maxX - minX,
                                  height: maxY - minY);
        }
    }

    public virtual bool Add(PointF point)
    {
        Points.Add(point);
        return true;
    }

    public override string ToString()
        => $"{base.ToString()}, Points: {string.Join(", ", Points)}";
}

public class PolylineFigure : PointsFigure
{
    public bool IsFilled { get; set; } = true;

    protected override void DrawShape(Graphics graphics, Pen pen)
    {
        if (IsClosed) {
            if (IsFilled) {
                using Brush brush = Brush;
                graphics.FillPolygon(brush, Points.ToArray());
            }
            graphics.DrawPolygon(pen, Points.ToArray());
        } else {
            graphics.DrawLines(pen, Points.ToArray());
        }
    }
}

public class CurveFigure : PointsFigure
{
    public bool IsFilled { get; set; } = true;

    public override RectangleF ShapeBounds
    {
        get {
            var bounds = base.ShapeBounds;
            const float inflateRate = 0.05f;
            bounds.Inflate(bounds.Width * inflateRate, bounds.Height * inflateRate);
            return bounds;
        }
    }

    protected override void DrawShape(Graphics graphics, Pen pen)
    {
        if (IsClosed) {
            if (IsFilled) {
                using Brush brush = Brush;
                graphics.FillClosedCurve(brush, Points.ToArray());
            }
            graphics.DrawClosedCurve(pen, Points.ToArray());
        } else {
            graphics.DrawCurve(pen, Points.ToArray());
        }
    }
}

public class FreeFormCurveFigure : PointsFigure
{
    const float minimumDistance = 10.0f;

    public override RectangleF ShapeBounds {
        get {
            var bounds = base.ShapeBounds;
            const float inflateRate = 0.05f;
            bounds.Inflate(bounds.Width * inflateRate, bounds.Height * inflateRate);
            return bounds;
        }
    }

    public override bool Add(PointF point)
    {
        if (point.IsValidForBezier(Points, minimumDistance)) {
            Points.Add(point);
            return true;
        }
        return false;
    }

    protected override void DrawShape(Graphics graphics, Pen pen)
    {
        if (IsClosed)
            GraphicHelper.DrawBezierPolygon(graphics, pen, Points);
        else
            GraphicHelper.DrawBezierLine(graphics, pen, Points);
    }
}

public static class GraphicHelper
{
    public static void DrawBezierPolygon(Graphics graphics, Pen pen, IList<PointF> points)
    {
        if (points.Count == 2) {
            graphics.DrawLines(pen, points.ToArray());
        } else if (points.Count >= 3) {
            var bezierPoints = points.ToBezierPolygon().ToArray();
            if (bezierPoints.Length >= 4)
                graphics.DrawBeziers(pen, bezierPoints);
        }
    }

    public static void DrawBezierLine(Graphics graphics, Pen pen, IList<PointF> points)
    {
        if (points.Count < 2)
            return;

        graphics.DrawLine(pen, points[0], points[1]);

        var bezierPoints = points.ToBezierPolyline().ToArray();
        if (bezierPoints.Length >= 4)
            graphics.DrawBeziers(pen, bezierPoints);

        //for (var index = 0; index < points.Length - 3; index++) {
        //    var bezierPoints = Geometry.ToBezier(points[index], points[index + 1], points[index + 2], points[index + 3]).ToArray();
        //    if (bezierPoints.Length >= 4)
        //        graphics.DrawBezier(pen, bezierPoints[0], bezierPoints[1], bezierPoints[2], bezierPoints[3]);
        //}

        if (points.Count > 2)
            graphics.DrawLine(pen, points[^2], points[^1]);
    }
}

public static class Geometry
{
    public static IEnumerable<PointF> ToBezier(PointF point1, PointF point2, PointF point3, PointF point4)
    {
        const float a = 1.0f / 3.0f / 2.0f;

        yield return point2;
        yield return point2.Add(point3.Subtract(point1).Multiply(a));
        yield return point3.Add(point2.Subtract(point4).Multiply(a));
        yield return point3;
    }
}

// C# 14 の拡張ブロック (extension) で、レシーバーの型ごとに拡張メンバーをまとめる
public static class GeometryExtensions
{
    extension(PointF point)
    {
        public float Distance(PointF other) => point.Subtract(other).Absolute();

        public PointF Add(SizeF size) => new(point.X + size.Width, point.Y + size.Height);

        public SizeF Subtract(PointF other) => new(point.X - other.X, point.Y - other.Y);

        public bool IsValidForBezier(IList<PointF> points, float minimumDistance)
            => points.Count <= 0 || points[^1].Distance(point) >= minimumDistance;
    }

    extension(SizeF size)
    {
        public SizeF Multiply(float rate) => new(size.Width * rate, size.Height * rate);

        public float Absolute() => (size.Width.Square() + size.Height.Square()).SquareRoot();
    }

    extension(float value)
    {
        public float Square() => value * value;

        public float SquareRoot() => MathF.Sqrt(value);
    }

    extension(IList<PointF> points)
    {
        public IEnumerable<PointF> ToBezierPolyline()
        {
            if (points.Count < 4) {
                foreach (var point in points)
                    yield return point;
            } else {
                const float a = 1.0f / 3.0f / 2.0f;
                yield return points[1];
                for (var index = 0; index < points.Count - 3; index++) {
                    yield return points[index + 1].Add(points[index + 2].Subtract(points[index + 0]).Multiply(a));
                    yield return points[index + 2].Add(points[index + 1].Subtract(points[index + 3]).Multiply(a));
                    yield return points[index + 2];
                }
            }
        }

        public IEnumerable<PointF> ToBezierPolygon()
        {
            if (points.Count < 3)
                return points;
            List<PointF> polygonPoints = [.. points, points[0], points[1], points[2]];
            return polygonPoints.ToBezierPolyline();
        }
    }
}
