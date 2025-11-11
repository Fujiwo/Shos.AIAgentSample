using System.ComponentModel;
using System.Drawing.Imaging;

namespace FCAICad
{
    public partial class View : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Model? Model { get; set; }

        public View()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;
        }

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

        void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopyToClipboard();
                e.Handled = true;
            }
        }

        public void CopyToClipboard()
        {
            if (Model is null || !Model.Any())
                return;

            var bounds = Model.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            // Add some padding around the figures
            const float padding = 10.0f;
            bounds.Inflate(padding, padding);

            var width = (int)Math.Ceiling(bounds.Width);
            var height = (int)Math.Ceiling(bounds.Height);

            // Create bitmap
            using var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.TranslateTransform(-bounds.X, -bounds.Y);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Model.ForEach(figure => figure.Draw(graphics));
            }

            // Create metafile
            using var metafile = CreateMetafile(bounds, width, height);

            // Copy both formats to clipboard
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Bitmap, bitmap);
            dataObject.SetData(DataFormats.EnhancedMetafile, metafile);
            Clipboard.SetDataObject(dataObject, true);
        }

        Metafile CreateMetafile(RectangleF bounds, int width, int height)
        {
            using var referenceGraphics = CreateGraphics();
            var hdc = referenceGraphics.GetHdc();
            try
            {
                var metafile = new Metafile(hdc, new Rectangle(0, 0, width, height), MetafileFrameUnit.Pixel, EmfType.EmfPlusDual);
                using (var graphics = Graphics.FromImage(metafile))
                {
                    graphics.Clear(Color.White);
                    graphics.TranslateTransform(-bounds.X, -bounds.Y);
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Model?.ForEach(figure => figure.Draw(graphics));
                }
                return metafile;
            }
            finally
            {
                referenceGraphics.ReleaseHdc(hdc);
            }
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
