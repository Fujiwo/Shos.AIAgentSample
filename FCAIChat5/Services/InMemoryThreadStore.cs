using System.Collections.Concurrent;
using System.Text.Json;

namespace FCAIChat.Services;

/// <summary>In-memory implementation of IThreadStore using JSON storage</summary>
public class InMemoryThreadStore : IThreadStore
{
    readonly ConcurrentDictionary<string, string> threadStorage = new();

    public Task SaveThreadAsync(string userKey, JsonElement serializedThread)
    {
        var json = serializedThread.GetRawText();
        threadStorage[userKey] = json;
        return Task.CompletedTask;
    }

    public Task<JsonElement?> GetThreadAsync(string userKey)
    {
        if (threadStorage.TryGetValue(userKey, out var json)) {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            return Task.FromResult<JsonElement?>(jsonElement);
        }
        return Task.FromResult<JsonElement?>(null);
    }

    public Task DeleteThreadAsync(string userKey)
    {
        threadStorage.TryRemove(userKey, out _);
        return Task.CompletedTask;
    }
}
