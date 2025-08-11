using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Adega.Data
{
    // Usada quando você rodar "dotnet ef ... -c AppDbContextPg"
    public class PgFactory : IDesignTimeDbContextFactory<AppDbContextPg>
    {
        public AppDbContextPg CreateDbContext(string[] args) {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddUserSecrets(typeof(PgFactory).Assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                   ?? cfg.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

            if (!conn.Contains("Host=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("A fábrica PG exige uma connection string de Postgres (Host=...).");

            var opt = new DbContextOptionsBuilder<AppDbContextPg>()
                .UseNpgsql(conn);

            return new AppDbContextPg(opt.Options);
        }
    }

    // Usada quando você rodar "dotnet ef ... -c AppDbContextSqlite"
    public class SqliteFactory : IDesignTimeDbContextFactory<AppDbContextSqlite>
    {
        public AppDbContextSqlite CreateDbContext(string[] args) {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddUserSecrets(typeof(SqliteFactory).Assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                   ?? cfg.GetConnectionString("DefaultConnection")
                   ?? "Data Source=Data/adega.db";

            if (!conn.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("A fábrica SQLite exige uma connection string de SQLite (Data Source=...).");

            var opt = new DbContextOptionsBuilder<AppDbContextSqlite>()
                .UseSqlite(conn);

            return new AppDbContextSqlite(opt.Options);
        }
    }
}
