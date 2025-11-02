namespace FCAIChat.Hubs
{
    using FCAIChat.AIAgents;
    using FCAIChat.Data;
    using FCAIChat.Services;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Concurrent;
    using System.Diagnostics;

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

        public async Task Clear()
        {
            var connectionId = Context.ConnectionId;
            if (chatAgents.TryRemove(connectionId, out var agent))
                agent.Dispose();
            await threadStore.DeleteThreadAsync(connectionId);

            dbContext.Messages.Clear();
            await dbContext.SaveChangesAsync();
        }

        public async Task SendMessage(string user, string prompt)
        {
            var connectionId = Context.ConnectionId;
            var chatAgent = await GetOrCreateChatAgentAsync(connectionId);

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

        private async Task<MyChatAgent> GetOrCreateChatAgentAsync(string connectionId)
        {
            if (chatAgents.TryGetValue(connectionId, out var existingAgent))
                return existingAgent;

            var agent = new MyChatAgent();
            // Try to restore thread from store
            var serializedThread = await threadStore.GetThreadAsync(connectionId);
            if (serializedThread.HasValue) {
                try {
                    agent.RestoreThread(serializedThread.Value);
                } catch (Exception ex) {
                    // Log the exception - thread restoration failed, will create new thread
                    Debug.WriteLine($"Thread restoration failed for connection {connectionId}: {ex.Message}");
                }
            }
            
            return chatAgents.GetOrAdd(connectionId, agent);
        }

        private async Task SaveThreadAsync(string connectionId, MyChatAgent chatAgent)
        {
            if (chatAgent.Thread is not null && chatAgent.Agent is not null) {
                var serializedThread = chatAgent.Thread.Serialize();
                await threadStore.SaveThreadAsync(connectionId, serializedThread);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            if (chatAgents.TryRemove(connectionId, out var agent))
                agent.Dispose();

            // Note: We keep the thread in storage for potential reconnection
            // To clean up old threads, implement a separate cleanup mechanism
            await base.OnDisconnectedAsync(exception);
        }
    }

    public static class EntityExtensions
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
            => dbSet.RemoveRange(dbSet);
    }
}
