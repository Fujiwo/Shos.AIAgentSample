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
            if (Model is null)
                return;
            foreach (var figure in Model)
                figure.Draw(e.Graphics);
        }
    }
}
