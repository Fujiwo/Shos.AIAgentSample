namespace FCAIChat.Hubs
{
    using FCAIChat.AIAgents;
    using FCAIChat.Data;
    using Microsoft.AspNetCore.SignalR;
    using System.Text.RegularExpressions;

    // Hub は呼び出しごとに生成される (Transient) ため、エージェントは ChatAgentManager (Singleton) から取得する
    public class ChatHub(MessagesDbContext dbContext, ChatAgentManager chatAgentManager) : Hub
    {
        // エージェントをユーザー単位で管理するためのキー (未ログインの場合は接続単位)
        string UserKey => Context.UserIdentifier ?? Context.ConnectionId;

        public async Task SendMessage(string user, string prompt)
        {
            await SendAsync(user, prompt);

            var (isForAgent, promptForAgent) = GetPromptForAgent(prompt);
            if (isForAgent) {
                var chatAgent = await chatAgentManager.AcquireAsync(UserKey, Context.ConnectionId);
                var response  = await chatAgent.GetResponseAsync(promptForAgent);
                await SendAsync(chatAgent.Name, response);
            }

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
            // ユーザーの接続がすべて切れたら、エージェント (と MCP サーバーのプロセス) を破棄する
            await chatAgentManager.ReleaseAsync(UserKey, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
