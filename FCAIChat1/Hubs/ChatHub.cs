namespace FCAIChat.Hubs
{
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            var createdAt = DateTime.UtcNow;
            createdAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
            await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
        }
    }
}
