using Microsoft.AspNetCore.SignalR;

namespace FCAIChat.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
    }
}
