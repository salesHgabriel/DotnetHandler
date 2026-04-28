using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetHandler.Tests.Handlers;

public class HandlerExecutionTests
{
    [Fact]
    public async Task Send_ExecutesHandlerAndReturnsResponse()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Handlers(h =>
                    h.Register<PingRequest, string>().HandledBy<PingHandler>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new PingRequest("hello"));

        Assert.Equal("Pong: hello", result);
    }

    [Fact]
    public async Task Send_ThrowsInvalidOperation_WhenHandlerNotRegistered()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app => { }));

        var dispatcher = provider.GetRequiredService<IDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.Send(new PingRequest("hello")));
    }
}
