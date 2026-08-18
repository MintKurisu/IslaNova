using IslaNova.Core.Application.Features.Property.Events;
using IslaNova.Core.Application.Interfaces.AI;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.AI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace IslaNova.Infrastructure.AI.IOC
{
    public static class ServiceRegistration
    {
        public static void AddAILayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configuration
            services.Configure<OpenAISettings>(config.GetSection("OpenAISettings"));
            #endregion

            #region Channel (in-memory queue for async vector sync)
            // Bounded channel to prevent unbounded memory growth under heavy load.
            // Capacity: 1000 events. If full, writes will wait (backpressure).
            var channel = Channel.CreateBounded<PropertyVectorEvent>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,  // Only PropertyVectorSyncService reads
                SingleWriter = false  // Multiple handlers can write
            });
            services.AddSingleton(channel);
            #endregion

            #region AI Services
            services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddScoped<IChatCompletionService, OpenAIChatCompletionService>();
            #endregion

            #region Background Services
            services.AddHostedService<PropertyVectorSyncService>();
            #endregion
        }
    }
}
