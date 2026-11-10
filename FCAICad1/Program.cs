namespace FCAICad;

class Program
{
    MainForm?            mainForm;
    readonly MyChatAgent chatAgent = new();

    /// <summary> The main entry point for the application.</summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        new Program().Run();
    }

    void Run()
    {
        mainForm = Toolbox.MainForm = new MainForm();
        mainForm.Prompted += OnPrompted;
        Application.Run(mainForm);

        // 終了処理 MCP クライアントを破棄
        // UI スレッドの同期コンテキストのまま待機するとデッドロックする恐れがあるため、スレッドプール上で破棄を完了させる
        Task.Run(() => chatAgent.DisposeAsync().AsTask()).GetAwaiter().GetResult();
    }

    async void OnPrompted(object? sender, string prompt)
    {
        // async void では例外がアプリケーションを停止させるため、すべての例外をここで処理する
        try {
            var response = await chatAgent.GetResponseAsync(prompt);
            mainForm?.SetResponse($"Agent: {response}{Environment.NewLine}");
        } catch (Exception ex) {
            mainForm?.SetResponse($"Error: {ex.Message}{Environment.NewLine}");
        } finally {
            mainForm?.OnPromptedEnd();
        }
    }
}
