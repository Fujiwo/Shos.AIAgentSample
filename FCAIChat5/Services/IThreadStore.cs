// 新: このステップで追加したファイル (会話の状態の保存先)
using System.Text.Json;

namespace FCAIChat.Services;

/// <summary>会話の状態 (AgentSession をシリアライズしたもの) を保存・取得するストアのインターフェイス</summary>
public interface IThreadStore
{
    /// <summary>ユーザーの会話の状態を保存する</summary>
    /// <param name="userKey">会話のキー (ログインしているユーザーの ID)</param>
    /// <param name="serializedThread">シリアライズした会話の状態 (JsonElement)</param>
    Task SaveThreadAsync(string userKey, JsonElement serializedThread);

    /// <summary>ユーザーの会話の状態を取得する</summary>
    /// <param name="userKey">会話のキー (ログインしているユーザーの ID)</param>
    /// <returns>シリアライズした会話の状態 (JsonElement)。保存されていなければ null</returns>
    Task<JsonElement?> GetThreadAsync(string userKey);

    /// <summary>ユーザーの会話の状態を削除する</summary>
    /// <param name="userKey">会話のキー (ログインしているユーザーの ID)</param>
    Task DeleteThreadAsync(string userKey);
}
