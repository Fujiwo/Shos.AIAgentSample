namespace FCAIChat.AIAgents
{
    /// <summary>
    /// ユーザーごとの AI エージェントを管理する (DI に Singleton として登録する)
    /// - SignalR の Hub は呼び出しごとに生成される (Transient) ため、エージェントは Hub ではなくここで保持する
    /// - ユーザーの最後の接続が切れたら、エージェント (と MCP サーバーのプロセス) を破棄する
    /// </summary>
    public sealed class ChatAgentManager : IAsyncDisposable
    {
        sealed class Entry(MyChatAgent agent)
        {
            public MyChatAgent     Agent         { get; } = agent;
            public HashSet<string> ConnectionIds { get; } = [];
        }

        readonly Dictionary<string, Entry> entries = [];
        readonly SemaphoreSlim             gate    = new(1, 1);

        /// <summary>ユーザーのエージェントを取得する。なければ作成する。</summary>
        public async Task<MyChatAgent> AcquireAsync(string userKey, string connectionId)
        {
            await gate.WaitAsync();
            try {
                if (!entries.TryGetValue(userKey, out var entry)) {
                    entry = new Entry(new MyChatAgent());
                    entries[userKey] = entry;
                }
                entry.ConnectionIds.Add(connectionId);
                return entry.Agent;
            } finally {
                gate.Release();
            }
        }

        /// <summary>接続の終了を通知する。ユーザーの接続がなくなったら、エージェントを破棄する。</summary>
        public async Task ReleaseAsync(string userKey, string connectionId)
        {
            MyChatAgent? agentToDispose = null;
            await gate.WaitAsync();
            try {
                if (entries.TryGetValue(userKey, out var entry) &&
                    entry.ConnectionIds.Remove(connectionId) && entry.ConnectionIds.Count == 0) {
                    entries.Remove(userKey);
                    agentToDispose = entry.Agent;
                }
            } finally {
                gate.Release();
            }

            if (agentToDispose is not null)
                await agentToDispose.DisposeAsync();
        }

        /// <summary>アプリケーション終了時に、すべてのエージェント (と MCP サーバーのプロセス) を破棄する</summary>
        public async ValueTask DisposeAsync()
        {
            List<MyChatAgent> agents;
            await gate.WaitAsync();
            try {
                agents = [.. entries.Values.Select(entry => entry.Agent)];
                entries.Clear();
            } finally {
                gate.Release();
            }

            foreach (var agent in agents)
                await agent.DisposeAsync();
        }
    }
}
