using System.Reflection;
using DotnetHandler.Abstractions;
using DotnetHandler.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Internal;

internal static class AssemblyScanner
{
    private static readonly HashSet<(Type Service, Type Implementation)> _registered = new();

    internal static void Scan(IServiceCollection services, Assembly assembly, HandlerWrapperRegistry registry)
    {
        var concreteTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var type in concreteTypes)
        {
            RegisterHandlers(services, assembly, registry, type);
            RegisterListeners(services, type);
        }
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, HandlerWrapperRegistry registry, Type type)
    {
        var handlerInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        foreach (var iface in handlerInterfaces)
        {
            var key = (iface, type);
            if (_registered.Contains(key)) continue;
            _registered.Add(key);
            services.AddScoped(iface, type);

            var args = iface.GetGenericArguments(); // [TRequest, TResponse]
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(args);
            var wrapper = (IRequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            registry.Register(args[0], wrapper);
        }
    }

    private static void RegisterListeners(IServiceCollection services, Type type)
    {
        var listenerInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEventListener<>));

        foreach (var iface in listenerInterfaces)
        {
            var key = (iface, type);
            if (_registered.Contains(key)) continue;
            _registered.Add(key);
            services.AddScoped(iface, type);
        }
    }
}
