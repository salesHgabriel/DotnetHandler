using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Tests.Helpers;

internal static class ServiceProviderFactory
{
    internal static IServiceProvider Build(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}
