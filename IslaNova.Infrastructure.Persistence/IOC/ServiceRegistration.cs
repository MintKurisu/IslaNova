using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IslaNova.Core.Domain.Interfaces.Base;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Repositories.UserInteraction;

namespace IslaNova.Infrastructure.Persistence.IOC
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IslaNovaContext>(options =>
                {
                    options.UseInMemoryDatabase("IslaNovaInMemoryDb");
                });
            }
            else
            {
                var currentDev = config.GetValue<string>("CurrentDev");
                var connectionString = config.GetConnectionString(currentDev ?? "Default");
                services.AddDbContext<IslaNovaContext>(
                    options =>
                    {
                        options.EnableSensitiveDataLogging();
                        options.UseNpgsql(
                            connectionString,
                            npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                                typeof(IslaNovaContext).Assembly.FullName)
                        );
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }
            #endregion

            #region Repositories IOC
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IImprovementRepository, ImprovementRepository>();
            services.AddScoped<IPropertyImprovementRepository, PropertyImprovementRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IPropertyImageRepository, PropertyImageRepository>();
            services.AddScoped<IPropertyTypeRepository, PropertyTypeRepository>();
            services.AddScoped<ISaleTypeRepository, SaleTypeRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            #endregion
        }
    }
}
