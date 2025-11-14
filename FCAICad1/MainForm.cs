namespace FCAICad;

public partial class MainForm : Form
{
    public event EventHandler<string>? Prompted;

    readonly Model model = new();
    readonly string[] buttonTexts = ["🤖", "🧠", "💡", "🧬", "📡"];

    public SizeF PaperSize => model.Size;

    public MainForm()
    {
        InitializeComponent();

        view.Model = model;
        model.Update += (_, figure) => {
            if (figure is null)
                view.Invalidate();
            else
                view.Draw(figure);
        };

        // Enable keyboard events for the form
        KeyPreview = true;
        KeyDown += OnFormKeyDown;
    }

    void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        // Forward Ctrl+C to the view for clipboard copy
        if (e.Control && e.KeyCode == Keys.C) {
            view.CopyToClipboard();
            e.Handled = true;
        }
    }

    public void Clear() => model.Clear();
    public void AddFigure(Figure figure) => model.Add(figure);

    public void AppendResponse(string response) => responseTextBox.Text = response;
    public void OnPromptedEnd() => promptButton.Enabled = true;

    void OnPromptButtonClick(object sender, EventArgs e)
    {
        var prompt = promptTextBox.Text;
        if (!string.IsNullOrWhiteSpace(prompt)) {
            promptButton.Enabled = false;
            AnimatePromptButton();
            Prompted?.Invoke(this, prompt);
        }

        void AnimatePromptButton()
        {
            Animator.Animate(500, frame => {
                if (promptButton.Enabled) {
                    promptButton.Text = buttonTexts[0];
                    return false;
                }
                promptButton.Text = (frame % buttonTexts.Length) switch {
                    1 => "🧠",
                    2 => "💡",
                    3 => "🧬",
                    4 => "📡",
                    _ => "🤖"
                };
                return true;
            });
        }
    }

    void OnClearButtonClick(object sender, EventArgs e) => model.Clear();
}
