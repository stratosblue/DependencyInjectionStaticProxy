using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DependencyInjectionServiceProxyExtensions
{
    #region Public 方法

    /// <summary>
    /// 将服务列表 <paramref name="services"/> 中的服务 <typeparamref name="TService"/> 替换为 <typeparamref name="TServiceImplementation"/> 实现<br/>
    /// 列表中的原始 <typeparamref name="TService"/> 可以通过 <see cref="IReplacedServiceAccessor{TService}"/> 进行访问<br/>
    /// 替换失败时会抛出异常
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TServiceImplementation"></typeparam>
    /// <param name="services"></param>
    /// <param name="serviceLifetime">新注入的 <typeparamref name="TService"/> 的生命周期，不指定时，自动延用 <paramref name="services"/> 中已存在的服务的生命周期（如存在多种周期则会抛出异常）</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection ProxyReplace<TService, TServiceImplementation>(this IServiceCollection services, ServiceLifetime? serviceLifetime = null)
        where TService : class
        where TServiceImplementation : class, TService
    {
        var replaceResult = TryProxyReplaceInternal<TService, TServiceImplementation>(services, serviceLifetime);
        switch (replaceResult)
        {
            case ProxyReplaceResult.Success:
                break;

            case ProxyReplaceResult.MultipleLifetimes:
                throw new InvalidOperationException($"Service of \"{typeof(TService)}\" has multiple lifetimes. Please clearly specify a lifetime for it.");

            case ProxyReplaceResult.Proxied:
                throw new InvalidOperationException($"The service \"{typeof(TService)}\" has been proxied.");

            case ProxyReplaceResult.ImplementationTypeSameAsTheService:
                throw new InvalidOperationException($"The service \"{typeof(TService)}\" has registed as itself.");

            case ProxyReplaceResult.None:
            default:
                throw new InvalidOperationException($"No service of \"{typeof(TService)}\" can replace.");
        }

        return services;
    }

    /// <summary>
    /// 尝试将服务列表 <paramref name="services"/> 中的服务 <typeparamref name="TService"/> 替换为 <typeparamref name="TServiceImplementation"/> 实现<br/>
    /// 列表中的原始 <typeparamref name="TService"/> 可以通过 <see cref="IReplacedServiceAccessor{TService}"/> 进行访问
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TServiceImplementation"></typeparam>
    /// <param name="services"></param>
    /// <param name="serviceLifetime">新注入的 <typeparamref name="TService"/> 的生命周期，不指定时，自动延用 <paramref name="services"/> 中已存在的服务的生命周期（如存在多种周期则替换失败）</param>
    /// <returns>是否替换成功</returns>
    [ExcludeFromCodeCoverage]
    public static bool TryProxyReplace<TService, TServiceImplementation>(this IServiceCollection services, ServiceLifetime? serviceLifetime = null)
        where TService : class
        where TServiceImplementation : class, TService
    {
        var replaceResult = TryProxyReplaceInternal<TService, TServiceImplementation>(services, serviceLifetime);
        return replaceResult == ProxyReplaceResult.Success;
    }

    #endregion Public 方法

    #region Internal 方法

    internal static ProxyReplaceResult TryProxyReplaceInternal<TService, TServiceImplementation>(IServiceCollection services, ServiceLifetime? serviceLifetime = null)
        where TService : class
        where TServiceImplementation : class, TService
    {
        if (services.Any(m => m.ServiceType == typeof(IReplacedServiceAccessor<TService>)))
        {
            return ProxyReplaceResult.Proxied;
        }

        var targetServiceDescriptors = services.Where(m => m.ServiceType == typeof(TService)).ToList();

        if (targetServiceDescriptors.Count == 0)
        {
            return ProxyReplaceResult.None;
        }

        if (targetServiceDescriptors.Any(m => m.ImplementationType is not null && m.ImplementationType == typeof(TService)))
        {
            return ProxyReplaceResult.ImplementationTypeSameAsTheService;
        }

        if (serviceLifetime is null)
        {
            var serviceLifetimes = targetServiceDescriptors.Select(m => m.Lifetime).Distinct().ToList();
            if (serviceLifetimes.Count > 1)
            {
                return ProxyReplaceResult.MultipleLifetimes;
            }
            serviceLifetime = serviceLifetimes[0];
        }

        foreach (var item in targetServiceDescriptors)
        {
            services.Remove(item);

            if (item.ImplementationInstance is { } implementationInstance)
            {
                var originAccessorServiceDescriptor = new ServiceDescriptor(serviceType: typeof(IReplacedServiceAccessor<TService>),
                                                                            instance: new ReplacedServiceAccessor<TService, TService>((TService)implementationInstance));

                services.Add(originAccessorServiceDescriptor);
            }
            else if (item.ImplementationFactory is { } implementationFactory)
            {
                var originAccessorServiceDescriptor = ServiceDescriptor.Describe(serviceType: typeof(IReplacedServiceAccessor<TService>),
                                                                                 implementationFactory: serviceProvider => new ReplacedServiceAccessor<TService, TService>((TService)implementationFactory(serviceProvider)),
                                                                                 lifetime: item.Lifetime);

                services.Add(originAccessorServiceDescriptor);
            }
            else if (item.ImplementationType is { } implementationType)
            {
                var originServiceDescriptor = ServiceDescriptor.Describe(serviceType: implementationType,
                                                                         implementationType: implementationType,
                                                                         lifetime: item.Lifetime);

                var originAccessorServiceDescriptor = ServiceDescriptor.Describe(serviceType: typeof(IReplacedServiceAccessor<TService>),
                                                                                 implementationType: typeof(ReplacedServiceAccessor<,>).MakeGenericType(typeof(TService), item.ImplementationType),
                                                                                 lifetime: item.Lifetime);

                services.Add(originServiceDescriptor);
                services.Add(originAccessorServiceDescriptor);
            }
            else
            {
                throw new InvalidOperationException($"Error ServiceDescriptor \"{item}\"");
            }
        }

        services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TServiceImplementation), serviceLifetime!.Value));
        return ProxyReplaceResult.Success;
    }

    #endregion Internal 方法

    #region Internal 枚举

    internal enum ProxyReplaceResult
    {
        None,

        Success,

        MultipleLifetimes,

        Proxied,

        ImplementationTypeSameAsTheService,
    }

    #endregion Internal 枚举

    #region Private 类

    private sealed class ReplacedServiceAccessor<TService, TOriginService>
        : IReplacedServiceAccessor<TService>
        where TService : class
        where TOriginService : class, TService
    {
        #region Public 属性

        public TService Service { get; }

        #endregion Public 属性

        #region Public 构造函数

        public ReplacedServiceAccessor(TOriginService originService)
        {
            Service = originService ?? throw new ArgumentNullException(nameof(originService));
        }

        #endregion Public 构造函数
    }

    #endregion Private 类
}

/// <summary>
/// 被替换的服务 <typeparamref name="TService"/> 的原始服务访问器
/// </summary>
/// <typeparam name="TService"></typeparam>
public interface IReplacedServiceAccessor<out TService>
    where TService : class
{
    #region Public 属性

    /// <summary>
    /// 容器中原始的 <typeparamref name="TService"/> 实例
    /// </summary>
    public TService Service { get; }

    #endregion Public 属性
}
