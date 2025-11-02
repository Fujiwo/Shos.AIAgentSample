using System.Collections.Concurrent;
using System.Text.Json;

namespace FCAIChat.Services;

/// <summary>In-memory implementation of IThreadStore using JSON storage</summary>
public class InMemoryThreadStore : IThreadStore
{
    readonly ConcurrentDictionary<string, string> threadStorage = new();

    public Task SaveThreadAsync(string connectionId, JsonElement serializedThread)
    {
        var json = serializedThread.GetRawText();
        threadStorage[connectionId] = json;
        return Task.CompletedTask;
    }

    public Task<JsonElement?> GetThreadAsync(string connectionId)
    {
        if (threadStorage.TryGetValue(connectionId, out var json)) {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            return Task.FromResult<JsonElement?>(jsonElement);
        }
        return Task.FromResult<JsonElement?>(null);
    }

    public Task DeleteThreadAsync(string connectionId)
    {
        threadStorage.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }
}
