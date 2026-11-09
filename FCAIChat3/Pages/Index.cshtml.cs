using FCAIChat.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<Message> Messages { get; private set; } = Enumerable.Empty<Message>();

        readonly MessagesDbContext _dbContext;
        readonly ILogger<IndexModel> _logger;

        public IndexModel(MessagesDbContext dbContext, ILogger<IndexModel> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try {
                Messages = await _dbContext.Messages
                                           .OrderBy(message => message.CreatedAt)
                                           .Select(message => new Message {
                                               Id = message.Id,
                                               UserName = message.UserName,
                                               Content = message.Content,
                                               CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)
                                           })
                                           .ToListAsync();
            } catch (SqlException ex) {
                _logger.LogError(ex, "Failed to load messages from SQL Server/LocalDB.");
                Messages = Enumerable.Empty<Message>();
            }
        }
    }
}
