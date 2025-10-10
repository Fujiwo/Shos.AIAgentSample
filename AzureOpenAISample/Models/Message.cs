namespace AzureOpenAISample.Models
{
    public class Message
    {
        public int Id { get; set; }
		public int PromptKind { get; set; }
		public string Owner { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
