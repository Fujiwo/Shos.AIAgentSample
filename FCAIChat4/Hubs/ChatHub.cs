// 新: エージェント (ChatAgentManager) 用
using FCAIChat.AIAgents;
using FCAIChat.Data;
using Microsoft.AspNetCore.SignalR;
// 新: 正規表現 (エージェント宛てかどうかの判定) 用
using System.Text.RegularExpressions;

namespace FCAIChat.Hubs;

// チャットの SignalR Hub (ブラウザーの chat.js と /chatHub でリアルタイムに通信する)
// 旧:
//public class ChatHub(MessagesDbContext dbContext) : Hub
// 新: ここから
// Hub は呼び出しごとに生成される (Transient) ため、エージェントは ChatAgentManager (Singleton) から取得する
public class ChatHub(MessagesDbContext dbContext, ChatAgentManager chatAgentManager) : Hub
// 新: ここまで
{
    // 新: ここから
    // エージェントをユーザー単位で管理するためのキー (未ログインの場合は接続単位)
    string UserKey => Context.UserIdentifier ?? Context.ConnectionId;
    // 新: ここまで

    // 旧: ここから
    //// クライアント (chat.js) から呼ばれる: 受け取ったメッセージを接続中の全クライアントに送信する
    //public async Task SendMessage(string user, string message)
    //{
    //    // 送信日時 (UTC)。クライアント側でローカル時刻に変換して表示する
    //    var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    //    // メッセージを DB に保存
    //    dbContext.Messages.Add(new() { UserName = user, Content = message, CreatedAt = createdAt });
    //    await dbContext.SaveChangesAsync();

    //    // 全クライアントの "ReceiveMessage" ハンドラーを呼び出す (日時は ISO 8601 形式の文字列にして送る)
    //    await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
    //}
    // 旧: ここまで

    // 新: ここから
    // メッセージを送信し、エージェント宛て ("@Agent" を含む) ならエージェントの応答も送信する
    public async Task SendMessage(string user, string prompt)
    {
        // まず、ユーザーのメッセージを DB に保存して全クライアントに送信
        await SendAsync(user, prompt);

        // エージェント宛てなら、そのユーザーのエージェントに問い合わせ、応答をエージェントからのメッセージとして送信
        var (isForAgent, promptForAgent) = GetPromptForAgent(prompt);
        if (isForAgent) {
            var chatAgent = await chatAgentManager.AcquireAsync(UserKey, Context.ConnectionId);
            var response  = await chatAgent.GetResponseAsync(promptForAgent);
            await SendAsync(chatAgent.Name, response);
        }

        // メッセージを DB に保存して全クライアントに送信
        async Task SendAsync(string user, string content)
        {
            var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            dbContext.Messages.Add(new Message() { UserName = user, Content = content, CreatedAt = createdAt });
            await dbContext.SaveChangesAsync();
            await Clients.All.SendAsync("ReceiveMessage", user, content, createdAt.ToString("o"));
        }

        // プロンプトがエージェント宛てかどうかを判定し、エージェント宛てなら宛先 ("@Agent") を取り除いたプロンプトを返す
        static (bool isForAgent, string prompt) GetPromptForAgent(string prompt)
        {
            // "@Agent" のような宛先が含まれていればエージェント宛て (大文字・小文字は区別しない)
            var chatAgentReference = new Regex(Regex.Escape($"@{MyChatAgent.AgentName}"), RegexOptions.IgnoreCase);
            return chatAgentReference.IsMatch(prompt)
                   ? (true, chatAgentReference.Replace(prompt, string.Empty).Trim())
                   : (false, string.Empty);
        }
    }

    // クライアントの接続が切れたときに SignalR から呼ばれる
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // ユーザーの接続がすべて切れたら、エージェント (と MCP サーバーのプロセス) を破棄する
        await chatAgentManager.ReleaseAsync(UserKey, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    // 新: ここまで
}
