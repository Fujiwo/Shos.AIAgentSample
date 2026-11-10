// 新: このステップで追加したファイル (メッセージを保存する DB の定義)
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Data;

// チャットのメッセージを保存する DB (SQL Server) にアクセスするための DbContext
public class MessagesDbContext(DbContextOptions<MessagesDbContext> options) : DbContext(options)
{
    // Messages テーブル
    public virtual DbSet<Message> Messages { get; set; }

    // 参考: 接続文字列を DbContext 自身に持たせる場合の書き方 (このアプリでは Program.cs の AddDbContext() で接続文字列を指定している)
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    const string connectionString = @"Server=(localdb)\mssqllocaldb;Database=aspnet-FCAIChat-ecb85b14-ebd2-4744-bec9-be8f021688b7;Trusted_Connection=True;MultipleActiveResultSets=true";
    //    optionsBuilder.UseSqlServer(connectionString);

    //    base.OnConfiguring(optionsBuilder);
    //}
}

// チャットのメッセージ 1 件分 (Messages テーブルの 1 行: ID・本文・送信者名・送信日時 (UTC))
public class Message
{
    public int      Id        { get; set; }
    public string   Content   { get; set; } = string.Empty;
    public string   UserName  { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
