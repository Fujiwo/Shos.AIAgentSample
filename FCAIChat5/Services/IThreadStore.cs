// 新: このステップで追加したファイル (会話の状態の保存先)
using System.Text.Json;

namespace FCAIChat.Services;

/// <summary>Interface for storing and retrieving AgentSession data</summary>
public interface IThreadStore
{
    /// <summary>Saves a serialized thread for a specific user</summary>
    /// <param name="userKey">The key of the conversation (the signed-in user ID)</param>
    /// <param name="serializedThread">The serialized thread as JsonElement</param>
    Task SaveThreadAsync(string userKey, JsonElement serializedThread);

    /// <summary>Retrieves a serialized thread for a specific user</summary>
    /// <param name="userKey">The key of the conversation (the signed-in user ID)</param>
    /// <returns>The serialized thread as JsonElement, or null if not found</returns>
    Task<JsonElement?> GetThreadAsync(string userKey);

    /// <summary>Deletes a stored thread for a specific user</summary>
    /// <param name="userKey">The key of the conversation (the signed-in user ID)</param>
    Task DeleteThreadAsync(string userKey);
}
