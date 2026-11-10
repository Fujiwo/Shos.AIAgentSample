using FCAIChat.AIAgents;
using FCAIChat.Data;
// 新: [Authorize] 用
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
// 新: ExecuteDeleteAsync() 用
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FCAIChat.Hubs;

// Hub は呼び出しごとに生成される (Transient) ため、エージェントは ChatAgentManager (Singleton) から取得する
// 新: ここから
// ログインしているユーザーだけが利用できる
[Authorize]
// 新: ここまで
public class ChatHub(MessagesDbContext dbContext, ChatAgentManager chatAgentManager) : Hub
{
    // エージェントをユーザー単位で管理するためのキー (未ログインの場合は接続単位)
    string UserKey => Context.UserIdentifier ?? Context.ConnectionId;
    // 新: ここから
    // 送信者名はクライアントから受け取らず、サーバー側の認証情報から取得する
    string UserName => Context.User?.Identity?.Name ?? "(unknown)";
    // 新: ここまで

    // 新: ここから
    // 会話とメッセージ履歴をクリアする
    public async Task Clear()
    {
        // 呼び出したユーザーの会話 (エージェントの状態) をリセット
        await chatAgentManager.ResetAsync(UserKey);

        // メッセージ履歴 (全ユーザー共通) を削除し、全クライアントに再表示を通知
        await dbContext.Messages.ExecuteDeleteAsync();
        await Clients.All.SendAsync("Cleared");
    }
    // 新: ここまで

    // メッセージを送信し、エージェント宛て ("@Agent" を含む) ならエージェントの応答も送信する
    // 旧:
    //public async Task SendMessage(string user, string prompt)
    // 新: 送信者名はサーバー側で取得するので、引数では受け取らない
    public async Task SendMessage(string prompt)
    {
        // 旧:
        //await SendAsync(user, prompt);
        // 新:
        await SendAsync(UserName, prompt);

        var (isForAgent, promptForAgent) = GetPromptForAgent(prompt);
        if (isForAgent) {
            var chatAgent = await chatAgentManager.AcquireAsync(UserKey, Context.ConnectionId);
            var response  = await chatAgent.GetResponseAsync(promptForAgent);
            // 新: エージェントの応答後に会話の状態を保存
            await chatAgentManager.SaveAsync(UserKey, chatAgent);
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
        // ユーザーの接続がすべて切れたら、会話を保存してエージェントを破棄する (再接続時に復元される)
        await chatAgentManager.ReleaseAsync(UserKey, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
