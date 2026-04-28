using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetHandler.Tests.Events;

[Collection("StaticState")]
public class EventTests
{
    [Fact]
    public async Task Publish_WithMultipleListeners_AllReceiveEvent()
    {
        EmailListener.Received.Clear();
        AuditListener.Received.Clear();

        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Events(e =>
                    e.Register<UserRegisteredEvent>()
                     .Subscribe<EmailListener>()
                     .Subscribe<AuditListener>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        await dispatcher.Publish(new UserRegisteredEvent("alice"));

        Assert.Contains("email:alice", EmailListener.Received);
        Assert.Contains("audit:alice", AuditListener.Received);
    }

    [Fact]
    public async Task Publish_WithNoListeners_DoesNotThrow()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app => { }));

        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var exception = await Record.ExceptionAsync(
            () => dispatcher.Publish(new UnhandledEvent("data")));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Publish_OnlyCallsListenersForMatchingEventType()
    {
        EmailListener.Received.Clear();

        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Events(e =>
                    e.Register<UserRegisteredEvent>()
                     .Subscribe<EmailListener>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        await dispatcher.Publish(new UnhandledEvent("other"));

        Assert.Empty(EmailListener.Received);
    }
}
