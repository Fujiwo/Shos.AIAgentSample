namespace FCAIChat.Hubs
{
    using FCAIChat.AIAgents;
    using FCAIChat.Data;
    using FCAIChat.Services;
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Concurrent;

    public class ChatHub : Hub
    {
        readonly MessagesDbContext dbContext;
        readonly IThreadStore threadStore;
        static readonly ConcurrentDictionary<string, MyChatAgent> chatAgents = new();

        public ChatHub(MessagesDbContext dbContext, IThreadStore threadStore)
        {
            this.dbContext = dbContext;
            this.threadStore = threadStore;
        }

        public async Task SendMessage(string user, string prompt)
        {
            var connectionId = Context.ConnectionId;
            var chatAgent = GetOrCreateChatAgent(connectionId);

            DateTime createdAt = GetDateTime();
            var message = new Message() { UserName = user, Content = prompt, CreatedAt = createdAt };

            await SendAsync(user, prompt, createdAt, message);

            var (isForAgent, promptForAgent) = GetPromptForAgent(prompt);
            if (isForAgent) {
                var responce = await chatAgent.GetResponseAsync(promptForAgent);
                
                // Save the thread after agent response
                await SaveThreadAsync(connectionId, chatAgent);
                
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

        private MyChatAgent GetOrCreateChatAgent(string connectionId)
        {
            return chatAgents.GetOrAdd(connectionId, _ => new MyChatAgent());
        }

        private async Task SaveThreadAsync(string connectionId, MyChatAgent chatAgent)
        {
            if (chatAgent.Thread is not null && chatAgent.Agent is not null)
            {
                var serializedThread = chatAgent.Thread.Serialize();
                await threadStore.SaveThreadAsync(connectionId, serializedThread);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            if (chatAgents.TryRemove(connectionId, out var agent))
            {
                agent.Dispose();
            }
            await threadStore.DeleteThreadAsync(connectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
