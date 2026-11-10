// 新: このステップで追加したファイル (メッセージを保存する DB の定義)
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Data;

public class MessagesDbContext(DbContextOptions<MessagesDbContext> options) : DbContext(options)
{
    public virtual DbSet<Message> Messages { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    const string connectionString = @"Server=(localdb)\mssqllocaldb;Database=aspnet-FCAIChat-ecb85b14-ebd2-4744-bec9-be8f021688b7;Trusted_Connection=True;MultipleActiveResultSets=true";
    //    optionsBuilder.UseSqlServer(connectionString);

    //    base.OnConfiguring(optionsBuilder);
    //}
}

public class Message
{
    public int      Id        { get; set; }
    public string   Content   { get; set; } = string.Empty;
    public string   UserName  { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
