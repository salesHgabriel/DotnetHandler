using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetHandler.Tests.Isolation;

[Collection("StaticState")]
public class ModuleIsolationTests
{
    [Fact]
    public async Task OnlyHandlers_Registered_EventsNotNeeded()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Handlers(h =>
                    h.Register<PingRequest, string>().HandledBy<PingHandler>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new PingRequest("isolated"));

        Assert.Equal("Pong: isolated", result);
    }

    [Fact]
    public async Task OnlyEvents_Registered_HandlerNotNeeded()
    {
        EmailListener.Received.Clear();

        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Events(e =>
                    e.Register<UserRegisteredEvent>().Subscribe<EmailListener>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        await dispatcher.Publish(new UserRegisteredEvent("dave"));

        Assert.Contains("email:dave", EmailListener.Received);
    }

    [Fact]
    public async Task OnlyPipeline_RegisteredWithHandler_Works()
    {
        RecordingBehavior<EchoRequest, string>.Log.Clear();

        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
            {
                app.Handlers(h =>
                    h.Register<EchoRequest, string>().HandledBy<EchoHandler>());
                app.Pipeline(p =>
                    p.Use<RecordingBehavior<EchoRequest, string>>());
            }));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new EchoRequest("pipe-only"));

        Assert.Equal("pipe-only", result);
        Assert.Equal(new[] { "before", "after" }, RecordingBehavior<EchoRequest, string>.Log);
    }

    [Fact]
    public async Task Handlers_WithoutPipeline_DoNotRequireBehaviors()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Handlers(h =>
                    h.Register<EchoRequest, string>().HandledBy<EchoHandler>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new EchoRequest("no-pipe"));

        Assert.Equal("no-pipe", result);
    }
}
