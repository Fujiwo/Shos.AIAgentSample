using System.ComponentModel;

namespace FCAICad
{
    public partial class View : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Model? Model { get; set; }

        public View() => InitializeComponent();

        public void Draw(Figure figure)
        {
            using var graphics = CreateGraphics();
            figure.Draw(graphics);
        }

        void OnPaint(object sender, PaintEventArgs e)
        {
            Model?.ForEach(figure => figure.Draw(e.Graphics));
#if DEBUG
            DrawBounds(e.Graphics);
#endif // DEBUG
        }

#if DEBUG
        void DrawBounds(Graphics graphics)
        {
            if (Model is null)
                return;
            using var pen = new Pen(Color.LightGray);
            Model?.ForEach(figure => graphics.DrawRectangle(pen, figure.Bounds));
            graphics.DrawRectangle(pen, Model.Bounds);
        }
#endif // DEBUG
    }
}
