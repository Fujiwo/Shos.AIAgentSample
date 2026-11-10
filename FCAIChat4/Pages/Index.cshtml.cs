using FCAIChat.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Pages;

// トップページ (チャット画面) のページモデル
public class IndexModel(MessagesDbContext dbContext, ILogger<IndexModel> logger) : PageModel
{
    // 画面に表示するメッセージの一覧
    public IEnumerable<Message> Messages { get; private set; } = [];

    // ページの表示時 (GET) に、DB に保存されたメッセージを送信日時の順に読み込む
    public async Task OnGetAsync()
    {
        try {
            // DB から読み込んだ日時は Kind が Unspecified になるので、UTC として扱うように指定し直す
            Messages = await dbContext.Messages
                                      .OrderBy(message => message.CreatedAt)
                                      .Select(message => new Message {
                                          Id        = message.Id,
                                          UserName  = message.UserName,
                                          Content   = message.Content,
                                          CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)
                                      })
                                      .ToListAsync();
        } catch (SqlException ex) {
            // DB (SQL Server/LocalDB) に接続できない場合は、ログを出力してメッセージなしで表示する
            logger.LogError(ex, "Failed to load messages from SQL Server/LocalDB.");
            Messages = [];
        }
    }
}
