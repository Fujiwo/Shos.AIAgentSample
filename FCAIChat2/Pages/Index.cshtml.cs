using FCAIChat.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<Message> Messages { get; private set; }

        readonly MessagesDbContext   _dbContext;
        readonly ILogger<IndexModel> _logger;

        public IndexModel(MessagesDbContext dbContext, ILogger<IndexModel> logger)
        {
            _dbContext = dbContext;
            Messages = dbContext.Messages.OrderBy(message => message.CreatedAt)
                                         .ToListAsync().Result
                                         .Select(message => new Message {
                                                                Id        = message.Id,
                                                                UserName  = message.UserName,
                                                                Content   = message.Content,
                                                                CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)
                                                            })
                                         .ToList();

            _logger = logger;
        }

        public void OnGet()
        {}
    }
}
