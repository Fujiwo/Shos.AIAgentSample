using System.Text.Json;

namespace FCAIChat.Services;

/// <summary>Interface for storing and retrieving AgentThread data</summary>
public interface IThreadStore
{
    /// <summary>Saves a serialized thread for a specific connection</summary>
    /// <param name="connectionId">The SignalR connection ID</param>
    /// <param name="serializedThread">The serialized thread as JsonElement</param>
    Task SaveThreadAsync(string connectionId, JsonElement serializedThread);

    /// <summary>Retrieves a serialized thread for a specific connection</summary>
    /// <param name="connectionId">The SignalR connection ID</param>
    /// <returns>The serialized thread as JsonElement, or null if not found</returns>
    Task<JsonElement?> GetThreadAsync(string connectionId);

    /// <summary>Deletes a stored thread for a specific connection</summary>
    /// <param name="connectionId">The SignalR connection ID</param>
    Task DeleteThreadAsync(string connectionId);
}
