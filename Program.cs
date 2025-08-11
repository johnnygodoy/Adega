using Adega.Data;
using Adega.Filters;
using Adega.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var culturaBR = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaBR;
CultureInfo.DefaultThreadCurrentUICulture = culturaBR;

var builder = WebApplication.CreateBuilder(args);

// 1) tenta ENV var
var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
// 2) tenta config / secrets
var cfgConn = builder.Configuration.GetConnectionString("DefaultConnection");

// caminho absoluto do SQLite (local x container)
var sqlitePath = Directory.Exists("/data")
    ? "/data/adega.db"
    : Path.Combine(builder.Environment.ContentRootPath, "Data", "adega.db");

var connStr = !string.IsNullOrWhiteSpace(envConn) ? envConn
            : !string.IsNullOrWhiteSpace(cfgConn) ? cfgConn
            : $"Data Source={sqlitePath}";

var isPg = connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase);

// registra o DbContext correto + migrations assembly da “casca”
if (isPg)
{
    builder.Services.AddDbContext<AppDbContext, AppDbContextPg>(opt =>
     opt.UseNpgsql(connStr, npg =>
     {
         npg.MigrationsAssembly(typeof(AppDbContextPg).Assembly.FullName);
         npg.EnableRetryOnFailure(
             maxRetryCount: 5,
             maxRetryDelay: TimeSpan.FromSeconds(10),
             errorCodesToAdd: null);
     })
     // aumenta o timeout padrão de comando
     .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
}
else
{
    builder.Services.AddDbContext<AppDbContext, AppDbContextSqlite>(opt =>
        opt.UseSqlite(connStr,
            x => x.MigrationsAssembly(typeof(AppDbContextSqlite).Assembly.FullName)));
}

// MVC + filtro + sessão
builder.Services.AddControllersWithViews(o => o.Filters.Add<LicencaValidaFilter>());
builder.Services.AddSession();
builder.Services.AddScoped<LicencaValidaFilter>();

var app = builder.Build();

// === MIGRATE + SEED ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // se for SQLite, garanta a pasta do arquivo antes
    if (!isPg)
    {
        var dir = Path.GetDirectoryName(sqlitePath)!;
        Directory.CreateDirectory(dir);
    }

    db.Database.Migrate();

    if (!db.ConfiguracoesSistema.Any())
    {
        db.ConfiguracoesSistema.Add(new ConfiguracaoSistema
        {
            DataInstalacao = DateTime.Today,
            LicencaValidaAte = DateTime.Today.AddDays(30)
        });
        db.SaveChanges();
    }
}
// === fim MIGRATE + SEED ===

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
