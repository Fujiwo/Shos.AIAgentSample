using Azure;
using Azure.AI.OpenAI;
using AzureOpenAISample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAISample.Pages
{
	public abstract class AIChatModel : PageModel
	{
		const int maximumMessageCount = 10 - 1;
		const string clearCommand = "clear";

		readonly AzureOpenAIContext _context;

		public AIChatModel(AzureOpenAIContext context) => _context = context;

		[BindProperty]
		public IList<Message> Messages { get; set; } = new List<Message>();

		[BindProperty]
		public string UserMessage { get; set; } = "";

		protected abstract string PageName { get; }
		protected abstract int PromptKind { get; }
		protected abstract string SystemMessage { get; }

		ChatMessage SystemChatMessage => new(ChatRole.System, SystemMessage);

		IList<ChatMessage> ChatMessages => new[] { SystemChatMessage }.Concat(Messages.Select(message => new ChatMessage(message.Owner.Equals("User") ? ChatRole.User : ChatRole.Assistant, message.Content))).ToList();

		ChatCompletionsOptions CurrentChatCompletionsOptions {
			get {
				var chatCompletionsOptions = new ChatCompletionsOptions {
					Temperature = (float)0.7,
					MaxTokens = 800,
					NucleusSamplingFactor = (float)0.95,
					FrequencyPenalty = 0,
					PresencePenalty = 0,
				};
				foreach (var chatMessage in ChatMessages)
					chatCompletionsOptions.Messages.Add(chatMessage);

				return chatCompletionsOptions;
			}
		}

		//public IActionResult OnGetPartial()
		//{
		//	return Partial("EnglishPartial");
		//}

		public async Task OnGetAsync() => await InitializeMessages();

		public async Task<IActionResult> OnPostAsync()
		{
			await InitializeMessages();

			if (UserMessage.ToLower().Equals(clearCommand)) {
				_context.Messages.RemoveRange(_context.Messages);
				await _context.SaveChangesAsync();
				return RedirectToPage($"./{PageName}");
			}

			var message = new Message { PromptKind = PromptKind, Owner = "User", Content = UserMessage };
			Messages.Add(message);

            var endPoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

            var client = new OpenAIClient(
				new Uri(endPoint),
				new AzureKeyCredential(apiKey));

			var chatCompletionsOptions = CurrentChatCompletionsOptions;

            // ### If streaming is selected
            //Response<StreamingChatCompletions> response = await client.GetChatCompletionsStreamingAsync(deploymentOrModelName: "AzureOpenAISample20230511", chatCompletionsOptions);
            //using StreamingChatCompletions streamingChatCompletions = response.Value;

            // ### If streaming is not selected
            const string deploymentName = "202406AzureAIModel";
            var responseWithoutStream = await client.GetChatCompletionsAsync(deploymentName, chatCompletionsOptions);

			var completions = responseWithoutStream.Value;
			if (completions.Choices.Count > 0) {
				var responseText = completions.Choices[0].Message.Content;
				_context.Messages.Add(message);
				_context.Messages.Add(new Message { PromptKind = PromptKind, Owner = "Assistant", Content = responseText });
				await _context.SaveChangesAsync();
			}
			return RedirectToPage($"./{PageName}");
		}

		async Task InitializeMessages()
		{
			if (_context.Messages != null)
				Messages = await _context.Messages
										 .Where(message => message.PromptKind == PromptKind)
										 .OrderByDescending(message => message.CreatedAt)
										 .Take(maximumMessageCount)
										 .OrderBy(message => message.CreatedAt)
										 .ToListAsync();
		}
	}
}