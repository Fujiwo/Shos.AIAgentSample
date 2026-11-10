// 新: このステップで追加したファイル (会話の状態の保存先)
using System.Collections.Concurrent;
using System.Text.Json;

namespace FCAIChat.Services;

/// <summary>IThreadStore のメモリー上の実装 (会話の状態を JSON の文字列で保存する。アプリケーションを終了すると消える)</summary>
public class InMemoryThreadStore : IThreadStore
{
    // ユーザーのキー → 会話の状態 (JSON の文字列)。複数のスレッドから同時に呼ばれても安全な ConcurrentDictionary を使う
    readonly ConcurrentDictionary<string, string> threadStorage = new();

    // 会話の状態を JSON の文字列にして保存する (同じユーザーの前の状態は上書きする)
    public Task SaveThreadAsync(string userKey, JsonElement serializedThread)
    {
        var json = serializedThread.GetRawText();
        threadStorage[userKey] = json;
        return Task.CompletedTask;
    }

    // 保存しておいた JSON の文字列を JsonElement に戻して返す (保存されていなければ null)
    public Task<JsonElement?> GetThreadAsync(string userKey)
    {
        if (threadStorage.TryGetValue(userKey, out var json)) {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            return Task.FromResult<JsonElement?>(jsonElement);
        }
        return Task.FromResult<JsonElement?>(null);
    }

    // 保存しておいた会話の状態を削除する
    public Task DeleteThreadAsync(string userKey)
    {
        threadStorage.TryRemove(userKey, out _);
        return Task.CompletedTask;
    }
}
