using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetHandler.Tests.Isolation;

public class AssemblyScannerIsolationTests
{
    [Fact]
    public async Task FromAssembly_TwoIndependentProviders_BothResolveHandlerWithoutInterference()
    {
        var provider1 = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app => app.FromAssembly(typeof(PingRequest).Assembly)));

        var provider2 = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app => app.FromAssembly(typeof(PingRequest).Assembly)));

        var dispatcher1 = provider1.GetRequiredService<IDispatcher>();
        var dispatcher2 = provider2.GetRequiredService<IDispatcher>();

        // Regression: a static _registered set in AssemblyScanner used to cause the second
        // provider's scan to be silently skipped, leaving its HandlerWrapperRegistry empty
        // and its DI container without the handler registration.
        var result1 = await dispatcher1.Send(new PingRequest("first"));
        var result2 = await dispatcher2.Send(new PingRequest("second"));

        Assert.Equal("Pong: first", result1);
        Assert.Equal("Pong: second", result2);
    }

    [Fact]
    public async Task FromAssembly_ScanningSameAssemblyTwiceOnSameRegistry_DoesNotDuplicateRegistrations()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
            {
                app.FromAssembly(typeof(PingRequest).Assembly);
                app.FromAssembly(typeof(PingRequest).Assembly);
            }));

        var handlers = provider.GetServices<IRequestHandler<PingRequest, string>>().ToList();

        Assert.Single(handlers);
    }
}
