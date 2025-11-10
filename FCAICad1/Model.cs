using System.Collections;

namespace FCAICad
{
    public class Model : IEnumerable<Figure>
    {
        public Vector2D Size = new Vector2D { X = 10000.0, Y = 10000.0 };

        public event EventHandler<Figure>? Update;

        readonly List<Figure> figures = new();

        public void Add(Figure figure)
        {
            figures.Add(figure);
            Update?.Invoke(this, figure);
        }

        public IEnumerator<Figure> GetEnumerator() => figures.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Vector2D
    {
        const double scale = 10.0;

        public static Vector2D Zero => new Vector2D { X = 0.0, Y = 0.0 };

        public required double X { get; init; }
        public required double Y { get; init; }

        public Point ToPoint() => new Point(ToInteger(X), ToInteger(Y));
        public static Vector2D FromPoint(Point point) => new Vector2D { X = ToDouble(point.X), Y = ToDouble(point.Y) };

        static int ToInteger(double value) => (int)(value / scale);
        static double ToDouble(int value) => value * scale;
    }

    public class Figure
    {
        public Color Color { get; set; } = Color.Navy;
        public float Width { get; set; } = 3.0f;

        public void Draw(Graphics graphics)
        {
            using Pen pen = new(Color, Width);
            DrawShape(graphics, pen);
        }

        protected virtual void DrawShape(Graphics graphics, Pen pen) {}
    }

    public class LineFigure : Figure
    {
        public required Vector2D Start { get; init; }
        public required Vector2D End   { get; init; }

        protected override void DrawShape(Graphics graphics, Pen pen)
            =>  graphics.DrawLine(pen, Start.ToPoint(), End.ToPoint());
    }
}
