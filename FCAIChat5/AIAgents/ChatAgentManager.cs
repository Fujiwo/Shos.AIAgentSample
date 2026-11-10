using FCAIChat.Services;

namespace FCAIChat.AIAgents
{
    /// <summary>
    /// ユーザーごとの AI エージェントを管理する (DI に Singleton として登録する)
    /// - SignalR の Hub は呼び出しごとに生成される (Transient) ため、エージェントは Hub ではなくここで保持する
    /// - ユーザーの最後の接続が切れたら、会話の状態を保存してエージェント (と MCP サーバーのプロセス) を破棄する
    /// - 再接続時には、保存しておいた会話の状態を復元する
    /// </summary>
    public sealed class ChatAgentManager(IThreadStore threadStore) : IAsyncDisposable
    {
        sealed class Entry(MyChatAgent agent)
        {
            public MyChatAgent     Agent         { get; } = agent;
            public HashSet<string> ConnectionIds { get; } = [];
        }

        readonly Dictionary<string, Entry> entries         = [];
        // 保存・破棄の途中のエージェント (再接続時は、保存が終わるのを待ってから復元する)
        readonly Dictionary<string, Task>  pendingReleases = [];
        readonly SemaphoreSlim             gate            = new(1, 1);

        /// <summary>ユーザーのエージェントを取得する。なければ作成し、保存済みの会話があれば復元する。</summary>
        public async Task<MyChatAgent> AcquireAsync(string userKey, string connectionId)
        {
            for (; ;) {
                Task? pendingRelease;
                await gate.WaitAsync();
                try {
                    if (!pendingReleases.TryGetValue(userKey, out pendingRelease)) {
                        if (!entries.TryGetValue(userKey, out var entry)) {
                            var agent = new MyChatAgent();
                            var serializedSession = await threadStore.GetThreadAsync(userKey);
                            if (serializedSession.HasValue)
                                agent.RestoreSession(serializedSession.Value);
                            entry = new Entry(agent);
                            entries[userKey] = entry;
                        }
                        entry.ConnectionIds.Add(connectionId);
                        return entry.Agent;
                    }
                } finally {
                    gate.Release();
                }
                // 前のエージェントの保存が終わるまで待ってから、やり直す (保存の失敗はここでは無視する)
                try {
                    await pendingRelease;
                } catch {
                }
            }
        }

        /// <summary>エージェントの会話の状態を保存する</summary>
        public async Task SaveAsync(string userKey, MyChatAgent agent)
        {
            var serializedSession = await agent.SerializeSessionAsync();
            if (serializedSession.HasValue)
                await threadStore.SaveThreadAsync(userKey, serializedSession.Value);
        }

        /// <summary>接続の終了を通知する。ユーザーの接続がなくなったら、会話を保存してエージェントを破棄する。</summary>
        public async Task ReleaseAsync(string userKey, string connectionId)
        {
            Task? release = null;
            await gate.WaitAsync();
            try {
                if (entries.TryGetValue(userKey, out var entry) &&
                    entry.ConnectionIds.Remove(connectionId) && entry.ConnectionIds.Count == 0) {
                    entries.Remove(userKey);
                    release = SaveAndDisposeAsync(entry.Agent);
                    pendingReleases[userKey] = release;
                }
            } finally {
                gate.Release();
            }

            if (release is null)
                return;
            try {
                await release;
            } finally {
                await gate.WaitAsync();
                try {
                    if (pendingReleases.TryGetValue(userKey, out var pending) && pending == release)
                        pendingReleases.Remove(userKey);
                } finally {
                    gate.Release();
                }
            }

            async Task SaveAndDisposeAsync(MyChatAgent agent)
            {
                try {
                    await SaveAsync(userKey, agent);
                } finally {
                    await agent.DisposeAsync();
                }
            }
        }

        /// <summary>ユーザーの会話をリセットする (エージェントを破棄し、保存した会話も削除する)</summary>
        public async Task ResetAsync(string userKey)
        {
            MyChatAgent? agentToDispose = null;
            await gate.WaitAsync();
            try {
                if (entries.Remove(userKey, out var entry))
                    agentToDispose = entry.Agent;
                await threadStore.DeleteThreadAsync(userKey);
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
