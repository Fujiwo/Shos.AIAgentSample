namespace FCAIChat.Hubs
{
    using FCAIChat.AIAgents;
    using FCAIChat.Data;
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub
    {
        readonly MessagesDbContext dbContext;
        readonly MyChatAgent chatAgent = new();

        public ChatHub(MessagesDbContext dbContext) => this.dbContext = dbContext;

        public async Task SendMessage(string user, string prompt)
        {
            DateTime createdAt = GetDateTime();
            var message = new Message() { UserName = user, Content = prompt, CreatedAt = createdAt };

            await SendAsync(user, prompt, createdAt, message);

            var (isForAgent, promptForAgent) = GetPromptForAgent(prompt);
            if (isForAgent) {
                var responce = await chatAgent.GetResponseAsync(promptForAgent);
                createdAt = GetDateTime();
                message = new Message() { UserName = chatAgent.Name, Content = responce, CreatedAt = createdAt };
                await SendAsync(chatAgent.Name, responce, createdAt, message);
            }

            static DateTime GetDateTime()
            {
                var createdAt = DateTime.UtcNow;
                createdAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
                return createdAt;
            }

            async Task SendAsync(string user, string prompt, DateTime createdAt, Message message)
            {
                dbContext.Messages.Add(message);
                await dbContext.SaveChangesAsync();
                await Clients.All.SendAsync("ReceiveMessage", user, prompt, createdAt.ToString("o"));
            }

            (bool isForAgent, string prompt) GetPromptForAgent(string prompt)
            {
                var chatAgentReference = $"@{chatAgent.Name}";
                return prompt.Contains(chatAgentReference, StringComparison.OrdinalIgnoreCase)
                       ? (true, prompt.Replace(chatAgentReference, ""))
                       : (false, string.Empty);
            }
        }
    }
}
