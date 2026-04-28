using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetHandler.Tests.Pipeline;

[Collection("StaticState")]
public class PipelineTests
{
    [Fact]
    public async Task Send_WithNoBehaviors_ExecutesHandler()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Handlers(h =>
                    h.Register<EchoRequest, string>().HandledBy<EchoHandler>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new EchoRequest("plain"));

        Assert.Equal("plain", result);
    }

    [Fact]
    public async Task Send_WithSingleBehavior_WrapsResponse()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
            {
                app.Handlers(h =>
                    h.Register<EchoRequest, string>().HandledBy<EchoHandler>());
                app.Pipeline(p =>
                    p.Use<PrefixBehavior>());
            }));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new EchoRequest("value"));

        Assert.Equal("[PREFIX]value", result);
    }

    [Fact]
    public async Task Send_WithTwoBehaviors_AppliesOuterFirst()
    {
        // Pipeline: Prefix (outer) → Suffix (inner) → Handler
        // Suffix wraps handler result: "value" → "value[SUFFIX]"
        // Prefix wraps that:          "value[SUFFIX]" → "[PREFIX]value[SUFFIX]"
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
            {
                app.Handlers(h =>
                    h.Register<EchoRequest, string>().HandledBy<EchoHandler>());
                app.Pipeline(p =>
                {
                    p.Use<PrefixBehavior>();
                    p.Use<SuffixBehavior>();
                });
            }));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new EchoRequest("value"));

        Assert.Equal("[PREFIX]value[SUFFIX]", result);
    }

    [Fact]
    public async Task Send_RecordingBehavior_LogsBeforeAndAfter()
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
        await dispatcher.Send(new EchoRequest("test"));

        Assert.Equal(new[] { "before", "after" }, RecordingBehavior<EchoRequest, string>.Log);
    }
}
