using IslaNova.Core.Application.Interfaces.Email;
using IslaNova.Core.Application.Interfaces.Storage;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Supabase;

namespace IslaNova.Infrastructure.Shared.IOC
{
    public static class ServiceRegistration
    {

        public static async Task AddSharedLayerIocAsync(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Supabase
            var url = config["SupabaseSettings:Url"]!;
            var key = config["SupabaseSettings:SecretKey"]!;
            var supabase = new Client(url, key);
            await supabase.InitializeAsync();
            services.AddSingleton(supabase);
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStorageService, SupabaseStorageService>();
            #endregion
        }

    }
}
