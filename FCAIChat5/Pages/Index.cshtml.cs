using FCAIChat.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Pages;

public class IndexModel(MessagesDbContext dbContext, ILogger<IndexModel> logger) : PageModel
{
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
}
