// 新: メッセージを保存する DbContext 用
using FCAIChat.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
// 新: ここから
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
// 新: ここまで

namespace FCAIChat.Pages;

// 旧:
//public class IndexModel : PageModel
// 新: DB からメッセージを読み込むため、MessagesDbContext を受け取る
public class IndexModel(MessagesDbContext dbContext, ILogger<IndexModel> logger) : PageModel
{
    // 旧: ここから
    //public void OnGet()
    //{}
    // 旧: ここまで

    // 新: ここから DB に保存されたメッセージを読み込む
    public IEnumerable<Message> Messages { get; private set; } = [];

    public async Task OnGetAsync()
    {
        try {
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
            logger.LogError(ex, "Failed to load messages from SQL Server/LocalDB.");
            Messages = [];
        }
    }
    // 新: ここまで
}
