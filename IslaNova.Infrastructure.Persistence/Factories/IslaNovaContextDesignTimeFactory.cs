using IslaNova.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IslaNova.Infrastructure.Persistence.Factories
{
    /// <summary>
    /// Design-time factory for EF Core tooling (dotnet ef migrations add/update).
    /// Used because Program.cs is NOT executed at design time.
    /// Reads the connection string from the ISLANOBA_DB_CONNECTION environment variable,
    /// or falls back to a local PostgreSQL default for development.
    /// </summary>
    public class IslaNovaContextDesignTimeFactory : IDesignTimeDbContextFactory<IslaNovaContext>
    {
        public IslaNovaContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ISLANOBA_DB_CONNECTION")
                ?? "Host=localhost;Port=5432;Database=IslaNovaDb;Username=postgres;Password=root";

            var optionsBuilder = new DbContextOptionsBuilder<IslaNovaContext>();

            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(IslaNovaContext).Assembly.FullName);
            });

            return new IslaNovaContext(optionsBuilder.Options);
        }
    }
}
