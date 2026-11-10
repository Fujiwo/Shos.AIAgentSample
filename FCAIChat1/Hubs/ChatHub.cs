using Microsoft.AspNetCore.SignalR;

namespace FCAIChat.Hubs;

// チャットの SignalR Hub (ブラウザーの chat.js と /chatHub でリアルタイムに通信する)
public class ChatHub : Hub
{
    // クライアント (chat.js) から呼ばれる: 受け取ったメッセージを接続中の全クライアントに送信する
    public async Task SendMessage(string user, string message)
    {
        // 送信日時 (UTC)。クライアント側でローカル時刻に変換して表示する
        var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        // 全クライアントの "ReceiveMessage" ハンドラーを呼び出す (日時は ISO 8601 形式の文字列にして送る)
        await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
    }
}
