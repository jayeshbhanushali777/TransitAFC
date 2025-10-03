using Microsoft.Extensions.DependencyInjection;
using TransitAFC.Shared.Infrastructure.Handlers;
using Microsoft.AspNetCore.Http;

namespace TransitAFC.Shared.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTokenForwarding(this IServiceCollection services)
        {
            // Add IHttpContextAccessor manually since we're in a class library
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<TokenForwardingHandler>();
            return services;
        }

        public static IHttpClientBuilder AddTokenForwarding(this IHttpClientBuilder builder)
        {
            return builder.AddHttpMessageHandler<TokenForwardingHandler>();
        }
    }
}