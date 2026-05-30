using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IslaNova.Core.Application.Behaviors;
using IslaNova.Core.Application.Interfaces.ChatMessage;
using IslaNova.Core.Application.Interfaces.Favorite;
using IslaNova.Core.Application.Interfaces.Feature;
using IslaNova.Core.Application.Interfaces.Offer;
using IslaNova.Core.Application.Interfaces.Property;
using IslaNova.Core.Application.Interfaces.PropertyManagement;
using IslaNova.Core.Application.Services.ChatMessage;
using IslaNova.Core.Application.Services.Favorite;
using IslaNova.Core.Application.Services.Feature;
using IslaNova.Core.Application.Services.Offer;
using IslaNova.Core.Application.Services.Property;
using IslaNova.Core.Application.Services.PropertyManagement;
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

            #region Services IOC
            services.AddTransient<IPropertyService, PropertyService>();
            services.AddTransient<IFavoriteService, FavoriteService>();
            services.AddTransient<IOfferService, OfferService>();
            services.AddTransient<IChatMessageService, ChatMessageService>();
            services.AddScoped<IImprovementService, ImprovementService>();
            services.AddScoped<IPropertyTypeService, PropertyTypeService>();
            services.AddScoped<ISaleTypeService, SaleTypeService>();
            #endregion
        }
    }
}
