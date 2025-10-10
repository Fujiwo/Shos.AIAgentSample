// 参考: ASP.NET Core での Entity Framework Core を使用した Razor Pages - チュートリアル | Microsoft Learn
// https://learn.microsoft.com/ja-jp/aspnet/core/data/ef-rp/intro?view=aspnetcore-7.0&tabs=visual-studio&WT.mc_id=email

using AzureOpenAISample.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AzureOpenAIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureOpenAIContext") ?? throw new InvalidOperationException("Connection string 'AzureOpenAIContext' not found.")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
} else {
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AzureOpenAIContext>();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
