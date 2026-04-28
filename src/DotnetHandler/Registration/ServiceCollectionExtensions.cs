using DotnetHandler.Abstractions;
using DotnetHandler.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Registration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotnetHandler(
        this IServiceCollection services,
        Action<ApplicationBuilder> configure)
    {
        var registry = new HandlerWrapperRegistry();
        services.AddSingleton(registry);
        services.AddScoped<IDispatcher, Dispatcher>();

        var builder = new ApplicationBuilder(services, registry);
        configure(builder);

        return services;
    }
}
