using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
            if (e.Control && e.KeyCode == Keys.C) {
                CopyToClipboard();
                e.Handled = true;
            }
        }

        public void CopyToClipboard()
        {
            if (Model is null || !Model.Any())
                return;

            var bounds = Model.Bounds;
            if (bounds.Width <= 0.0f || bounds.Height <= 0.0f)
                return;

            // Add some padding around the figures
            const float padding = 10.0f;
            bounds.Inflate(padding, padding);

            Size size = new(width: Round(bounds.Width), height: Round(bounds.Height));

            try {
                using var bitmap = CreateBitmap  (bounds, size);
                var metafile     = CreateMetafile(bounds, size);
                var dataObject = new DataObject();
                dataObject.SetData(DataFormats.Bitmap, bitmap);
                dataObject.SetData(DataFormats.EnhancedMetafile, metafile);
                Clipboard.SetDataObject(dataObject, true);
            } catch {
            }

            static int Round(float value) => (int)Math.Ceiling(value);
        }

        Bitmap CreateBitmap(RectangleF bounds, Size size)
        {
            var bitmap = new Bitmap(width: size.Width, height: size.Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            graphics.TranslateTransform(-bounds.X, -bounds.Y);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Model?.ForEach(figure => figure.Draw(graphics));
            return bitmap;
        }

        Metafile CreateMetafile(RectangleF bounds, Size size)
        {
            using var referenceGraphics = CreateGraphics();
            var hdc = referenceGraphics.GetHdc();
            try {
                var metafile = new Metafile(hdc, new Rectangle(new Point(), size), MetafileFrameUnit.Pixel, EmfType.EmfPlusDual);
                using var graphics = Graphics.FromImage(metafile);
                graphics.Clear(Color.White);
                graphics.TranslateTransform(-bounds.X, -bounds.Y);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Model?.ForEach(figure => figure.Draw(graphics));
                return metafile;
            } finally {
                referenceGraphics.ReleaseHdc(hdc);
            }
        }

        static class NativeMethods
        {
            [DllImport("gdi32.dll", ExactSpelling = true)]
            internal static extern IntPtr CopyEnhMetaFile(IntPtr hEmfSrc, IntPtr hEmfDest);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteEnhMetaFile(IntPtr hEmf);
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
