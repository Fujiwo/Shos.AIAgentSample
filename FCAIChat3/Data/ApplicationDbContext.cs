using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FCAIChat.Data;

// ログイン情報 (ASP.NET Core Identity のユーザーやロールなど) を保存する DB にアクセスするための DbContext
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options);
