// 新: エージェント (ChatAgentManager) 用
using FCAIChat.AIAgents;
using FCAIChat.Data;
using Microsoft.AspNetCore.SignalR;
// 新: 正規表現 (エージェント宛てかどうかの判定) 用
using System.Text.RegularExpressions;

namespace FCAIChat.Hubs;

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
    //public async Task SendMessage(string user, string message)
    //{
    //    var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    //    dbContext.Messages.Add(new() { UserName = user, Content = message, CreatedAt = createdAt });
    //    await dbContext.SaveChangesAsync();

    //    await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
    //}
    // 旧: ここまで

    // 新: ここから
    // メッセージを送信し、エージェント宛て ("@Agent" を含む) ならエージェントの応答も送信する
    public async Task SendMessage(string user, string prompt)
    {
        await SendAsync(user, prompt);

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

        static (bool isForAgent, string prompt) GetPromptForAgent(string prompt)
        {
            // "@Agent" のような宛先が含まれていればエージェント宛て (大文字・小文字は区別しない)
            var chatAgentReference = new Regex(Regex.Escape($"@{MyChatAgent.AgentName}"), RegexOptions.IgnoreCase);
            return chatAgentReference.IsMatch(prompt)
                   ? (true, chatAgentReference.Replace(prompt, string.Empty).Trim())
                   : (false, string.Empty);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // ユーザーの接続がすべて切れたら、エージェント (と MCP サーバーのプロセス) を破棄する
        await chatAgentManager.ReleaseAsync(UserKey, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    // 新: ここまで
}
