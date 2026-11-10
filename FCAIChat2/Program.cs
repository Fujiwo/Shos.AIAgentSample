using FCAIChat.Data;
using FCAIChat.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Web アプリケーションのビルダーを作成 (設定・ログ・DI コンテナーなどを準備する)
var builder = WebApplication.CreateBuilder(args);

// DI コンテナーにサービスを登録する
// appsettings.json から DB の接続文字列 (DefaultConnection) を取得
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// ログイン情報 (Identity) を保存する DbContext を登録 (SQL Server を使う)
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
// 新: メッセージを保存する DbContext を登録
builder.Services.AddDbContext<MessagesDbContext>(options => options.UseSqlServer(connectionString));
// 開発時に、DB 関連のエラー (マイグレーションの未適用など) の詳しい情報を表示する
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// ASP.NET Core Identity (ユーザー登録・ログイン) を使う
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
// Razor Pages を使う
builder.Services.AddRazorPages();
// SignalR (サーバーとブラウザー間のリアルタイム通信) を使う
builder.Services.AddSignalR();

// 登録したサービスを使ってアプリケーションを生成
var app = builder.Build();

// HTTP リクエストを処理するパイプライン (ミドルウェア) を構成する
if (app.Environment.IsDevelopment()) {
    // 開発時は、未適用のマイグレーションをブラウザーから適用できるようにする
    app.UseMigrationsEndPoint();
} else {
    // 本番では、例外が発生したらエラー ページ (/Error) を表示する
    app.UseExceptionHandler("/Error");
    // HSTS (ブラウザーに HTTPS の使用を強制する) を有効にする。既定の有効期間は 30 日。本番環境に合わせて変更を検討すること (https://aka.ms/aspnetcore-hsts を参照)
    app.UseHsts();
}

// HTTP のリクエストを HTTPS にリダイレクトし、ルーティングと認可を有効にする
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// 静的ファイル (wwwroot) と Razor Pages のエンドポイントを登録
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
// チャットの SignalR Hub (ChatHub) を /chatHub に登録 (chat.js はこの URL に接続する)
app.MapHub<ChatHub>("/chatHub");

// アプリケーションを実行
app.Run();
