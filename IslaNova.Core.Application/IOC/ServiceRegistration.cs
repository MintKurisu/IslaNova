using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IslaNova.Core.Application.Behaviors;
using System.Reflection;

namespace IslaNova.Core.Application.IOC
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayerIOC(this IServiceCollection services)
        {
            #region Configurations
            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            services.AddMediatR(opt => opt.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            #endregion

        }
    }
}
