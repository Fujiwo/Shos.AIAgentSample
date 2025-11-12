using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FCAICad
{
    public partial class View : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Model? Model { get; set; }

        ClipboardHelper clipboardHelper = new();

        public View()
        {
            InitializeComponent();
            Paint += OnPaint;
            KeyDown += OnKeyDown;
        }

        public void Draw(Figure figure)
        {
            using var graphics = CreateGraphics();
            figure.Draw(graphics);
        }

        public void CopyToClipboard() => clipboardHelper.CopyToClipboard(Model, this);

        void OnPaint(object? _, PaintEventArgs e)
        {
            Model?.ForEach(figure => figure.Draw(e.Graphics));
#if DEBUG
            DrawBounds(e.Graphics);
#endif // DEBUG
        }

        void OnKeyDown(object? _, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C) {
                CopyToClipboard();
                e.Handled = true;
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

        class ClipboardHelper
        {
            //Metafile? metafile = null;

            //Metafile? Metafile
            //{
            //    get { return metafile; }
            //    set {
            //        if (value != metafile) {
            //            metafile?.Dispose();
            //            metafile = value;
            //        }
            //    }
            //}

            public void CopyToClipboard(Model? model, Control control)
            {
                if (model is null || !model.Any())
                    return;

                var bounds = model.Bounds;
                if (bounds.Width <= 0.0f || bounds.Height <= 0.0f)
                    return;

                // Add some padding around the figures
                const float padding = 10.0f;
                bounds.Inflate(padding, padding);

                Size size = new(width: Round(bounds.Width), height: Round(bounds.Height));

                try {
                    using var bitmap = CreateBitmap(model, bounds, size);
                    //Metafile = CreateMetafile(model, control, bounds, size);

                    //if (Metafile?.GetHenhmetafile() != IntPtr.Zero) {
                    //    var dataObject = new DataObject();
                    //    dataObject.SetData(DataFormats.EnhancedMetafile, false, Metafile);
                    //    dataObject.SetData(DataFormats.Bitmap, false, bitmap);
                    //    Clipboard.SetDataObject(dataObject, true);
                    //} else {
                        // Fallback to bitmap only if metafile copy failed
                        Clipboard.SetImage(bitmap);
                    //}
                } catch (ExternalException ex) {
                    // Handle clipboard access exceptions
                    System.Diagnostics.Debug.WriteLine($"Clipboard access failed: {ex.Message}");
                } catch (Exception ex) {
                    // Handle other exceptions
                    System.Diagnostics.Debug.WriteLine($"Failed to copy to clipboard: {ex.Message}");
                }

                static int Round(float value) => (int)Math.Ceiling(value);
            }

            Bitmap CreateBitmap(Model? model, RectangleF bounds, Size size)
            {
                var bitmap = new Bitmap(width: size.Width, height: size.Height);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.Clear(Color.White);
                graphics.TranslateTransform(-bounds.X, -bounds.Y);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                model?.ForEach(figure => figure.Draw(graphics));
                return bitmap;
            }

            Metafile CreateMetafile(Model? model, Control control, RectangleF bounds, Size size)
            {
                using var referenceGraphics = control.CreateGraphics();
                var hdc = referenceGraphics.GetHdc();
                try {
                    var metafile = new Metafile(hdc, new Rectangle(new Point(), size), MetafileFrameUnit.Pixel, EmfType.EmfPlusDual);
                    // Draw to the metafile and dispose the graphics to finalize the metafile content
                    using (var graphics = Graphics.FromImage(metafile)) {
                        graphics.Clear(Color.White);
                        graphics.TranslateTransform(-bounds.X, -bounds.Y);
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        model?.ForEach(figure => figure.Draw(graphics));
                    }
                    // Graphics is now disposed, metafile content is finalized
                    return metafile;
                } finally {
                    referenceGraphics.ReleaseHdc(hdc);
                }
            }
        }
    }
}
