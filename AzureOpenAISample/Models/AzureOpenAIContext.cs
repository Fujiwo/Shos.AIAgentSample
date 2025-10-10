using Microsoft.EntityFrameworkCore;

namespace AzureOpenAISample.Models
{
    public class AzureOpenAIContext : DbContext
    {
        public AzureOpenAIContext (DbContextOptions<AzureOpenAIContext> options) : base(options)
        {}

        public DbSet<Message> Messages { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>().ToTable("Message");
        }
    }
}
