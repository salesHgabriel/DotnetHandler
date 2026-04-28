using System.Reflection;
using DotnetHandler.Core;
using DotnetHandler.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Registration;

public sealed class ApplicationBuilder
{
    private readonly IServiceCollection _services;
    private readonly HandlerWrapperRegistry _registry;

    internal ApplicationBuilder(IServiceCollection services, HandlerWrapperRegistry registry)
    {
        _services = services;
        _registry = registry;
    }

    public ApplicationBuilder Handlers(Action<HandlerBuilder> configure)
    {
        configure(new HandlerBuilder(_services, _registry));
        return this;
    }

    public ApplicationBuilder Events(Action<EventBuilder> configure)
    {
        configure(new EventBuilder(_services));
        return this;
    }

    public ApplicationBuilder Pipeline(Action<PipelineBuilder> configure)
    {
        configure(new PipelineBuilder(_services));
        return this;
    }

    public ApplicationBuilder FromAssembly(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
            AssemblyScanner.Scan(_services, assembly, _registry);

        return this;
    }
}
