namespace FCAICad;

// アプリケーションの起点: メイン フォームと AI エージェントをつなぐ
class Program
{
    // メイン フォーム (CAD の画面) と、CAD を操作する AI エージェント
    MainForm?            mainForm;
    readonly MyChatAgent chatAgent = new();

    /// <summary>アプリケーションのメイン エントリ ポイント</summary>
    [STAThread]
    static void Main()
    {
        // 高 DPI 設定や既定のフォントなど、アプリケーションの構成を初期化する
        // 構成の変更方法は https://aka.ms/applicationconfiguration を参照
        ApplicationConfiguration.Initialize();
        new Program().Run();
    }

    void Run()
    {
        // メイン フォームを作成して Toolbox にも渡し (AI のツールから描画できるようにする)、
        // プロンプトが入力されたときのイベントを購読してから、フォームを表示する (閉じるまで戻らない)
        mainForm = Toolbox.MainForm = new MainForm();
        mainForm.Prompted += OnPrompted;
        Application.Run(mainForm);

        // 終了処理 MCP クライアントを破棄
        // UI スレッドの同期コンテキストのまま待機するとデッドロックする恐れがあるため、スレッドプール上で破棄を完了させる
        Task.Run(() => chatAgent.DisposeAsync().AsTask()).GetAwaiter().GetResult();
    }

    // プロンプトが入力されたら AI エージェントに送り、応答を応答欄に表示する
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
