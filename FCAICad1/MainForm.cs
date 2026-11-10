namespace FCAICad;

// メイン フォーム: 図形を表示するビュー、プロンプトの入力欄、AI の応答欄を持つ
public partial class MainForm : Form
{
    // プロンプトが入力されたときに発生する (Program が受け取って AI エージェントに渡す)
    public event EventHandler<string>? Prompted;

    // 描かれた図形を保持するモデル
    readonly Model model = new();
    // AI の応答を待つ間、プロンプト ボタンに順に表示する絵文字 (先頭は通常時の表示)
    readonly string[] buttonTexts = ["🤖", "🧠", "💡", "🧬", "📡"];

    // 用紙 (作図領域) のサイズ
    public SizeF PaperSize => model.Size;

    public MainForm()
    {
        InitializeComponent();

        // ビューにモデルを設定し、モデルが変化したらビューを更新する
        // (図形が追加されたらその図形だけを描き、クリアされたら (figure が null) 全体を再描画する)
        view.Model = model;
        model.Update += (_, figure) => {
            if (figure is null)
                view.Invalidate();
            else
                view.Draw(figure);
        };

        // 子コントロールより先に、フォームでキー入力を受け取れるようにする
        KeyPreview = true;
        KeyDown += OnFormKeyDown;
    }

    void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+C で、ビューの図形を画像としてクリップボードにコピーする
        if (e.Control && e.KeyCode == Keys.C) {
            view.CopyToClipboard();
            e.Handled = true;
        }
    }

    // AI エージェントのツール呼び出しは UI スレッド以外から行われることがあるため、UI スレッドで実行する
    public void Clear() => InvokeOnUIThread(model.Clear);
    public void AddFigure(Figure figure) => InvokeOnUIThread(() => model.Add(figure));

    // AI の応答を応答欄に表示する
    public void SetResponse(string response) => responseTextBox.Text = response;

    // UI スレッド以外から呼ばれた場合は、Invoke で UI スレッドに処理を移して実行する
    void InvokeOnUIThread(Action action)
    {
        if (InvokeRequired)
            Invoke(action);
        else
            action();
    }
    // AI の応答が終わったら、プロンプト ボタンを再び押せるようにする (ボタンのアニメーションも止まる)
    public void OnPromptedEnd() => promptButton.Enabled = true;

    // プロンプト ボタン: 入力内容を Prompted イベントで通知し、応答を待つ間はボタンを無効にしてアニメーションさせる
    void OnPromptButtonClick(object sender, EventArgs e)
    {
        var prompt = promptTextBox.Text;
        if (!string.IsNullOrWhiteSpace(prompt)) {
            promptButton.Enabled = false;
            AnimatePromptButton();
            Prompted?.Invoke(this, prompt);
        }

        // 500 ミリ秒ごとにボタンの絵文字を切り替え、ボタンが有効に戻ったら元の表示にして終了する
        void AnimatePromptButton()
        {
            Animator.Animate(500, frame => {
                if (promptButton.Enabled) {
                    promptButton.Text = buttonTexts[0];
                    return false;
                }
                promptButton.Text = buttonTexts[frame % buttonTexts.Length];
                return true;
            });
        }
    }

    // クリア ボタン: すべての図形を消去する
    void OnClearButtonClick(object sender, EventArgs e) => model.Clear();
}
