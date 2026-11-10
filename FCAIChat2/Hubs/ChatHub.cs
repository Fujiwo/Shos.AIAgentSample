using FCAIChat.Data;
using Microsoft.AspNetCore.SignalR;

namespace FCAIChat.Hubs;

public class ChatHub(MessagesDbContext dbContext) : Hub
{
    public async Task SendMessage(string user, string message)
    {
        var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        dbContext.Messages.Add(new() { UserName = user, Content = message, CreatedAt = createdAt });
        await dbContext.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
    }
}
