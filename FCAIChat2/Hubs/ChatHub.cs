// 新: メッセージを保存する DbContext 用
using FCAIChat.Data;
using Microsoft.AspNetCore.SignalR;

namespace FCAIChat.Hubs;

// 旧:
//public class ChatHub : Hub
// 新: メッセージを DB に保存するため、MessagesDbContext を受け取る
public class ChatHub(MessagesDbContext dbContext) : Hub
{
    public async Task SendMessage(string user, string message)
    {
        var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        // 新: ここから メッセージを DB に保存
        dbContext.Messages.Add(new() { UserName = user, Content = message, CreatedAt = createdAt });
        await dbContext.SaveChangesAsync();
        // 新: ここまで

        await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
    }
}
