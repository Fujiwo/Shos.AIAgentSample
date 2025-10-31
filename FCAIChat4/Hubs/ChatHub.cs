namespace FCAIChat.Hubs
{
    using FCAIChat.Data;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;

    public class ChatHub : Hub
    {
        readonly MessagesDbContext dbContext;

        public ChatHub(MessagesDbContext dbContext) => this.dbContext = dbContext;

        public async Task SendMessage(string user, string message)
        {
            var createdAt = DateTime.UtcNow;
            createdAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);

            dbContext.Messages.Add(new () { UserName = user, Content = message, CreatedAt = createdAt });
            await dbContext.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
        }
    }
}
