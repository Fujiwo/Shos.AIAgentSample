namespace FCAICad;

class Program
{
    MainForm?   mainForm;
    MyChatAgent chatAgent = new();

    /// <summary> The main entry point for the application.</summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        new Program().RunAsync().Wait();
    }

    async Task RunAsync()
    {
        mainForm = Toolbox.MainForm = new MainForm();
        mainForm.Prompted += OnPrompted;
        Application.Run(mainForm);
        await chatAgent.DisposeAsync();
    }

    async void OnPrompted(object? sender, string prompt)
    {
        var response = await chatAgent.GetResponseAsync(prompt);
        mainForm?.AppendResponse($"Agent: {response}{Environment.NewLine}");
        mainForm?.OnPromptedEnd();
    }
}
