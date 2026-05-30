using IslaNova.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IslaNova.Core.Application.Interfaces.Email;
using IslaNova.Core.Domain.Settings;

namespace IslaNova.Infrastructure.Shared.IOC
{
    public static class ServiceRegistration
    {

        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            #endregion
        }

    }
}
