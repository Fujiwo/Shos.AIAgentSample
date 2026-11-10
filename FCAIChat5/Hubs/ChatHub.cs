using FCAIChat.AIAgents;
using FCAIChat.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FCAIChat.Hubs;

// ログインしているユーザーだけが利用できる
[Authorize]
public class ChatHub(MessagesDbContext dbContext, ChatAgentManager chatAgentManager) : Hub
{
    // ログインユーザーの ID (エージェントと会話の状態をユーザー単位で管理するためのキー)
    string UserKey  => Context.UserIdentifier ?? Context.ConnectionId;
    // 送信者名はクライアントから受け取らず、サーバー側の認証情報から取得する
    string UserName => Context.User?.Identity?.Name ?? "(unknown)";

    public async Task Clear()
    {
        // 呼び出したユーザーの会話 (エージェントの状態) をリセット
        await chatAgentManager.ResetAsync(UserKey);

        // メッセージ履歴 (全ユーザー共通) を削除し、全クライアントに再表示を通知
        await dbContext.Messages.ExecuteDeleteAsync();
        await Clients.All.SendAsync("Cleared");
    }

    public async Task SendMessage(string prompt)
    {
        var chatAgent = await chatAgentManager.AcquireAsync(UserKey, Context.ConnectionId);

        await SendAsync(UserName, prompt);

        var (isForAgent, promptForAgent) = GetPromptForAgent(prompt);
        if (isForAgent) {
            var response = await chatAgent.GetResponseAsync(promptForAgent);

            // エージェントの応答後に会話の状態を保存
            await chatAgentManager.SaveAsync(UserKey, chatAgent);

            await SendAsync(chatAgent.Name, response);
        }

        async Task SendAsync(string user, string content)
        {
            var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            dbContext.Messages.Add(new Message() { UserName = user, Content = content, CreatedAt = createdAt });
            await dbContext.SaveChangesAsync();
            await Clients.All.SendAsync("ReceiveMessage", user, content, createdAt.ToString("o"));
        }

        (bool isForAgent, string prompt) GetPromptForAgent(string prompt)
        {
            // "@Agent" のような宛先が含まれていればエージェント宛て (大文字・小文字は区別しない)
            var chatAgentReference = new Regex(Regex.Escape($"@{chatAgent.Name}"), RegexOptions.IgnoreCase);
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
