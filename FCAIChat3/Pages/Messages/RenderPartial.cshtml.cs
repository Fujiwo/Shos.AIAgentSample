using FCAIChat.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FCAIChat.Pages.Messages
{
    public class RenderPartialModel : PageModel
    {
        // Query から複合型をバインドして受け取る（SupportsGet = true）
        [BindProperty(SupportsGet = true)]
        public Message Message { get; set; } = new();

        public void OnGet()
        {
            // Message.CreatedAt が Unspecified の場合は UTC として扱う（Z を付けるため）
            if (Message.CreatedAt.Kind == DateTimeKind.Unspecified)
                Message.CreatedAt = DateTime.SpecifyKind(Message.CreatedAt, DateTimeKind.Utc);
        }
    }
}
