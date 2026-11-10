using System.Collections;

namespace FCAICad;

// CAD のモデル: 描かれた図形の一覧を保持し、変化をイベントで通知する
// (AI のツール (Toolbox) が MainForm 経由で図形を追加し、View がそれを表示する)
public class Model : IEnumerable<Figure>
{
    // 用紙 (作図領域) のサイズ
    public SizeF Size { get; } = new(width: 3000.0f, height: 2000.0f);

    // 図形が追加されたとき (引数はその図形) と、クリアされたとき (引数は null) に発生する
    public event EventHandler<Figure?>? Update;

    readonly List<Figure> figures = [];

    // すべての図形を囲む外接矩形 (図形が無ければ空)
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

    // 図形を追加して通知する (追加した図形はデバッグ出力にも書き出す)
    public void Add(Figure figure)
    {
        figures.Add(figure);
        Update?.Invoke(this, figure);
        figure.Log();
    }

    // すべての図形を消去して通知する
    public void Clear()
    {
        figures.Clear();
        Update?.Invoke(this, null);
    }

    // foreach などで図形を列挙できるようにする
    public IEnumerator<Figure> GetEnumerator() => figures.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// 図形の基底クラス: 色と線幅を持ち、外接矩形の計算と描画を行う
public abstract class Figure
{
    public Color Color { get; set; } = Color.Black;
    // 色名 (例: "Red") で色を設定する (AI のツールからは色名で指定される)
    public string ColorName { set => Color = Color.FromName(value); }
    // 塗りつぶし用のブラシ (取得するたびに作られるので、使う側で破棄する)
    protected Brush Brush => new SolidBrush(Color);

    // 線幅
    public float LineWidth { get; set; } = 10.0f;

    // 線幅を含めた外接矩形 (線は輪郭を中心に描かれるので、線幅の半分だけ広げる)
    public RectangleF Bounds
    {
        get {
            var bounds = ShapeBounds;
            bounds.Inflate(LineWidth / 2.0f, LineWidth / 2.0f);
            return bounds;
        }
    }

    // 線幅を含まない、図形そのものの外接矩形 (派生クラスで実装する)
    public abstract RectangleF ShapeBounds { get; }

    // 色と線幅からペンを作り、派生クラスの DrawShape で描画する
    public void Draw(Graphics graphics)
    {
        using Pen pen = new(Color, LineWidth);
        DrawShape(graphics, pen);
    }

    // 図形の内容を表す文字列 (ツールの結果として AI に返したり、デバッグ出力に使ったりする)
    public override string ToString()
        => $"{GetType().Name} (Color: {Color.Name}, LineWidth: {LineWidth})";

    // 図形の形を描画する (派生クラスでオーバーライドする)
    protected virtual void DrawShape(Graphics graphics, Pen pen) { }
}

// 直線 (始点と終点)
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

// 矩形 (IsFilled が true なら塗りつぶす)
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

// 角丸矩形 (Radius は角の丸みの幅方向・高さ方向の半径)
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

// 円 (中心と半径)
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

// 楕円 (中心と X 方向・Y 方向の半径)
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

// 点列で表される図形の基底クラス (折れ線・多角形・曲線・自由曲線)
public abstract class PointsFigure : Figure
{
    // C# 14 の field キーワードで、自動生成されるバッキングフィールドを参照する
    // 設定時は Add で 1 点ずつ追加する (派生クラスが追加する点を選べるようにするため)
    public IList<PointF> Points
    {
        get;
        set {
            field.Clear();
            value.ForEach(point => Add(point));
        }
    } = [];

    // 閉じた図形にするかどうか
    public bool IsClosed { get; set; } = false;

    // すべての点を囲む外接矩形
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

    // 点を追加する。追加したら true を返す (派生クラスで、追加しない点を判定できる)
    public virtual bool Add(PointF point)
    {
        Points.Add(point);
        return true;
    }

    public override string ToString()
        => $"{base.ToString()}, Points: {string.Join(", ", Points)}";
}

// 折れ線 (IsClosed が true なら多角形。塗りつぶしは閉じた場合のみ)
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

// 点列を通る滑らかな曲線 (カーディナル スプライン)。IsClosed が true なら閉曲線 (塗りつぶしは閉じた場合のみ)
public class CurveFigure : PointsFigure
{
    public bool IsFilled { get; set; } = true;

    // 曲線は点列の外側に膨らむことがあるため、外接矩形を少し (5%) 広げる
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

// 点列をベジエ曲線でつないだ自由曲線 (塗りつぶしは無し)
public class FreeFormCurveFigure : PointsFigure
{
    // 前の点からこの距離未満の点は追加しない (点が密集すると曲線が乱れるため)
    const float minimumDistance = 10.0f;

    // 曲線は点列の外側に膨らむことがあるため、外接矩形を少し (5%) 広げる
    public override RectangleF ShapeBounds {
        get {
            var bounds = base.ShapeBounds;
            const float inflateRate = 0.05f;
            bounds.Inflate(bounds.Width * inflateRate, bounds.Height * inflateRate);
            return bounds;
        }
    }

    // 前の点に近すぎる点は追加しない
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

// 点列をベジエ曲線で描くためのヘルパー
public static class GraphicHelper
{
    // 点列を通る閉じたベジエ曲線を描く (2 点なら直線)
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

    // 点列を通る開いたベジエ曲線を描く (最初と最後の区間は直線で描く)
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

// 幾何計算
public static class Geometry
{
    // 連続する 4 点から、point2 と point3 の間をつなぐ 3 次ベジエ曲線の 4 つの制御点を求める
    // (Catmull-Rom スプラインをベジエ曲線に変換する方法。点の前後の傾きから制御点を決める)
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
        // 2 点間の距離
        public float Distance(PointF other) => point.Subtract(other).Absolute();

        // 点をベクトル (size) の分だけ移動した点
        public PointF Add(SizeF size) => new(point.X + size.Width, point.Y + size.Height);

        // other から point へのベクトル
        public SizeF Subtract(PointF other) => new(point.X - other.X, point.Y - other.Y);

        // 点列の最後の点から minimumDistance 以上離れていれば、点列に追加してよい
        public bool IsValidForBezier(IList<PointF> points, float minimumDistance)
            => points.Count <= 0 || points[^1].Distance(point) >= minimumDistance;
    }

    extension(SizeF size)
    {
        // ベクトルを rate 倍する
        public SizeF Multiply(float rate) => new(size.Width * rate, size.Height * rate);

        // ベクトルの長さ
        public float Absolute() => (size.Width.Square() + size.Height.Square()).SquareRoot();
    }

    extension(float value)
    {
        // 2 乗
        public float Square() => value * value;

        // 平方根
        public float SquareRoot() => MathF.Sqrt(value);
    }

    extension(IList<PointF> points)
    {
        // 点列を、Graphics.DrawBeziers に渡す制御点の列に変換する
        // (2 番目から最後から 2 番目までの点を、隣り合う点ごとに Catmull-Rom 方式のベジエ曲線でつなぐ。4 点未満ならそのまま返す)
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

        // 閉じた曲線用: 先頭の 3 点を末尾に足して一周つなげてから、ToBezierPolyline で変換する
        public IEnumerable<PointF> ToBezierPolygon()
        {
            if (points.Count < 3)
                return points;
            List<PointF> polygonPoints = [.. points, points[0], points[1], points[2]];
            return polygonPoints.ToBezierPolyline();
        }
    }
}
