using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FCAICad;

// モデルの図形を描画するビュー (ユーザー コントロール)。Ctrl+C で図形を画像としてクリップボードにコピーできる
public partial class View : UserControl
{
    // 表示するモデル (実行時に設定するので、デザイナーが生成するコードには含めない)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Model? Model { get; set; }

    // クリップボードへのコピーを担当するヘルパー
    readonly ClipboardHelper clipboardHelper = new();

    public View()
    {
        InitializeComponent();
        Paint += OnPaint;
        KeyDown += OnKeyDown;
    }

    // 追加された図形 1 つだけを描画する (全体を再描画しなくて済む)
    public void Draw(Figure figure)
    {
        using var graphics = CreateGraphics();
        figure.Draw(graphics);
#if DEBUG
        DrawBounds(graphics, figure);
#endif // DEBUG
    }

    // モデルの図形を画像としてクリップボードにコピーする
    public void CopyToClipboard() => clipboardHelper.CopyToClipboard(Model, this);

    // 再描画が必要になったら、モデルのすべての図形を描画する
    void OnPaint(object? _, PaintEventArgs e)
    {
        Model?.ForEach(figure => figure.Draw(e.Graphics));
#if DEBUG
        DrawBounds(e.Graphics);
#endif // DEBUG
    }

    // ビューにフォーカスがあるときの Ctrl+C でもコピーできるようにする
    void OnKeyDown(object? _, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.C) {
            CopyToClipboard();
            e.Handled = true;
        }
    }

    // デバッグ ビルドでのみ、確認用に図形とモデル全体の外接矩形を薄い灰色で描画する
#if DEBUG
    static readonly Color boundsColor = Color.LightGray;

    // すべての図形とモデル全体の外接矩形を描画する
    void DrawBounds(Graphics graphics)
    {
        if (Model is null)
            return;
        using var pen = new Pen(boundsColor);
        Model?.ForEach(figure => graphics.DrawRectangle(pen, figure.Bounds));
        graphics.DrawRectangle(pen, Model?.Bounds ?? RectangleF.Empty);
    }

    // 指定した図形の外接矩形を描画する
    void DrawBounds(Graphics graphics, Figure figure)
    {
        using var pen = new Pen(boundsColor);
        graphics.DrawRectangle(pen, figure.Bounds);
    }
#endif // DEBUG

    // モデルの図形を画像にしてクリップボードにコピーする
    // (メタファイル (EMF) でコピーする処理はコメントアウトして残してあり、現在はビットマップだけを使う)
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

        // すべての図形を囲む範囲を画像にしてコピーする (図形が無ければ何もしない)
        public void CopyToClipboard(Model? model, Control control)
        {
            if (model is null || !model.Any())
                return;

            var bounds = model.Bounds;
            if (bounds.Width <= 0.0f || bounds.Height <= 0.0f)
                return;

            // 図形の周りに余白を付ける
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
                    // ビットマップ画像としてコピーする (メタファイルを使う場合は、メタファイルを作れなかったときの代替になる)
                    Clipboard.SetImage(bitmap);
                //}
            } catch (ExternalException ex) {
                // 他のアプリケーションがクリップボードを使用中などで、アクセスに失敗した場合
                System.Diagnostics.Debug.WriteLine($"Clipboard access failed: {ex.Message}");
            } catch (Exception ex) {
                // その他の例外 (コピーに失敗してもアプリケーションは止めない)
                System.Diagnostics.Debug.WriteLine($"Failed to copy to clipboard: {ex.Message}");
            }

            // 図形が欠けないよう、小数点以下を切り上げて整数にする
            static int Round(float value) => (int)Math.Ceiling(value);
        }

        // 白い背景のビットマップに、bounds の左上が原点になるよう平行移動して図形を描く
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

        // メタファイル (EMF+ と EMF の両形式を含む) に図形を描く (現在は未使用)
        Metafile CreateMetafile(Model? model, Control control, RectangleF bounds, Size size)
        {
            using var referenceGraphics = control.CreateGraphics();
            // メタファイルの作成に必要な参照用のデバイス コンテキストを取得する (finally で必ず解放する)
            var hdc = referenceGraphics.GetHdc();
            try {
                var metafile = new Metafile(hdc, new Rectangle(new Point(), size), MetafileFrameUnit.Pixel, EmfType.EmfPlusDual);
                // メタファイルに描画し、Graphics を破棄することでメタファイルの内容を確定させる
                using (var graphics = Graphics.FromImage(metafile)) {
                    graphics.Clear(Color.White);
                    graphics.TranslateTransform(-bounds.X, -bounds.Y);
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    model?.ForEach(figure => figure.Draw(graphics));
                }
                // ここでは Graphics が破棄済みなので、メタファイルの内容は確定している
                return metafile;
            } finally {
                referenceGraphics.ReleaseHdc(hdc);
            }
        }
    }
}
