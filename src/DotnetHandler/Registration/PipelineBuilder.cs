using DotnetHandler.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Registration;

public sealed class PipelineBuilder
{
    private readonly IServiceCollection _services;

    internal PipelineBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>Register a closed pipeline behavior, e.g. Use&lt;MyBehavior&gt;().</summary>
    public PipelineBuilder Use<TBehavior>() where TBehavior : class
    {
        var behaviorType = typeof(TBehavior);
        RegisterBehaviorType(behaviorType);
        return this;
    }

    /// <summary>
    /// Register an open-generic pipeline behavior, e.g. Use(typeof(LoggingBehavior&lt;,&gt;)).
    /// Required because C# does not allow open generics as type arguments.
    /// </summary>
    public PipelineBuilder Use(Type behaviorType)
    {
        RegisterBehaviorType(behaviorType);
        return this;
    }

    private void RegisterBehaviorType(Type behaviorType)
    {
        if (behaviorType.IsGenericTypeDefinition)
        {
            _services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        }
        else
        {
            var closedInterfaces = behaviorType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

            foreach (var iface in closedInterfaces)
                _services.AddScoped(iface, behaviorType);
        }
    }
}
