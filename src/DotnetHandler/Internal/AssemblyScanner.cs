using System.Reflection;
using DotnetHandler.Abstractions;
using DotnetHandler.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Internal;

internal static class AssemblyScanner
{
    internal static void Scan(IServiceCollection services, Assembly assembly, HandlerWrapperRegistry registry)
    {
        var concreteTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var type in concreteTypes)
        {
            RegisterHandlers(services, assembly, registry, type);
            RegisterListeners(services, registry, type);
        }
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, HandlerWrapperRegistry registry, Type type)
    {
        var handlerInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        foreach (var iface in handlerInterfaces)
        {
            if (!registry.TryMarkScanned(iface, type)) continue;
            services.AddScoped(iface, type);

            var args = iface.GetGenericArguments(); // [TRequest, TResponse]
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(args);
            var wrapper = (IRequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            registry.Register(args[0], wrapper);
        }
    }

    private static void RegisterListeners(IServiceCollection services, HandlerWrapperRegistry registry, Type type)
    {
        var listenerInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEventListener<>));

        foreach (var iface in listenerInterfaces)
        {
            if (!registry.TryMarkScanned(iface, type)) continue;
            services.AddScoped(iface, type);
        }
    }
}
