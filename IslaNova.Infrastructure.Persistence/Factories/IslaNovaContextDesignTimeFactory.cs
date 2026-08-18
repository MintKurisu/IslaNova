using IslaNova.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IslaNova.Infrastructure.Persistence.Factories
{
    /// <summary>
    /// Design-time factory for EF Core tooling (dotnet ef migrations add/update).
    /// Used because Program.cs is NOT executed at design time.
    /// Reads the connection string from the ISLANOBA_DB_CONNECTION environment variable,
    /// or falls back to a local PostgreSQL default for development.
    /// </summary>
    public class IslaNovaContextDesignTimeFactory
      : IDesignTimeDbContextFactory<IslaNovaContext>
    {
        public IslaNovaContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    "appsettings.Development.json",
                    optional: true)
                .AddEnvironmentVariables()
                .Build();

            var currentDev = configuration["CurrentDev"];

            var connectionString = configuration.GetConnectionString(
                currentDev ?? "Default"
            );

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string for '{currentDev}' was not found."
                );
            }

            var optionsBuilder = new DbContextOptionsBuilder<IslaNovaContext>();

            optionsBuilder.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(
                        typeof(IslaNovaContext).Assembly.FullName
                    );
                });

            return new IslaNovaContext(optionsBuilder.Options);
        }
    }
}
