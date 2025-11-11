namespace FCAICad;

public partial class MainForm : Form
{
    public event EventHandler<string>? Prompted;

    Model model = new();

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
        promptButton.Enabled = false;

        var prompt = promptTextBox.Text;
        if (!string.IsNullOrWhiteSpace(prompt))
            Prompted?.Invoke(this, prompt);
    }

    void OnClearButtonClick(object sender, EventArgs e) => model.Clear();
}
