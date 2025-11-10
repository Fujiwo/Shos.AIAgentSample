namespace FCAICad;

public partial class MainForm : Form
{
    public event EventHandler<string>? Prompted;

    Model model = new();

    public Vector2D PaperSize => model.Size;

    public MainForm()
    {
        InitializeComponent();

        view.Model = model;
        model.Update += (_, figure) => view.Draw(figure);
    }

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
}
