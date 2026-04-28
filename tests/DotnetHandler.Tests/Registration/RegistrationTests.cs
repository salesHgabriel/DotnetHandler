using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetHandler.Tests.Registration;

public class RegistrationTests
{
    [Fact]
    public async Task FluentRegistration_HandlerAndEvent_WorkTogether()
    {
        EmailListener.Received.Clear();

        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
            {
                app.Handlers(h =>
                    h.Register<PingRequest, string>().HandledBy<PingHandler>());

                app.Events(e =>
                    e.Register<UserRegisteredEvent>().Subscribe<EmailListener>());
            }));

        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var handlerResult = await dispatcher.Send(new PingRequest("reg-test"));
        await dispatcher.Publish(new UserRegisteredEvent("bob"));

        Assert.Equal("Pong: reg-test", handlerResult);
        Assert.Contains("email:bob", EmailListener.Received);
    }

    [Fact]
    public async Task AssemblyRegistration_DiscoversBothHandlersAndListeners()
    {
        EmailListener.Received.Clear();

        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.FromAssembly(typeof(PingHandler).Assembly)));

        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var result = await dispatcher.Send(new PingRequest("assembly"));
        await dispatcher.Publish(new UserRegisteredEvent("carl"));

        Assert.Equal("Pong: assembly", result);
        Assert.Contains("email:carl", EmailListener.Received);
    }

    [Fact]
    public async Task CombinedRegistration_FluentPlusPipeline_Works()
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
        var result = await dispatcher.Send(new EchoRequest("combined"));

        Assert.Equal("combined", result);
        Assert.NotEmpty(RecordingBehavior<EchoRequest, string>.Log);
    }
}
