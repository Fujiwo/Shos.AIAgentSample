using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Data;

public class MessagesDbContext : DbContext
{
    public virtual DbSet<Message> Messages { get; set; }

    //public MessagesDbContext()
    //{}

    public MessagesDbContext(DbContextOptions<MessagesDbContext> options)
        : base(options)
    {}

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    const string connectionString = @"Server=(localdb)\mssqllocaldb;Database=aspnet-FCAIChat-ecb85b14-ebd2-4744-bec9-be8f021688b7;Trusted_Connection=True;MultipleActiveResultSets=true";
    //    optionsBuilder.UseSqlServer(connectionString);

    //    base.OnConfiguring(optionsBuilder);
    //}
}

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
