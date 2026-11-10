using FCAIChat.AIAgents;
using FCAIChat.Data;
using FCAIChat.Hubs;
// 新: 会話の状態を保存するストア (IThreadStore) 用
using FCAIChat.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<MessagesDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// 新: ここから
// 会話の状態を保存するストアを登録 (ユーザーごとの会話の状態をメモリーに保存する)
builder.Services.AddSingleton<IThreadStore, InMemoryThreadStore>();
// 新: ここまで
// ユーザーごとのエージェントを管理するサービスを登録
// (SignalR の Hub は呼び出しごとに生成されるため、エージェントは Singleton のサービスで保持する)
builder.Services.AddSingleton<ChatAgentManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<ChatHub>("/chatHub");

app.Run();
