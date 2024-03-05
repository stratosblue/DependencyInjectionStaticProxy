using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionStaticProxy.Test;

[TestClass]
public class GenericTest
{
    #region Public 方法

    #region Type

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldReplaceTypeClassSuccess(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(Counter), typeof(DeriveCounter), serviceLifetime));
            services.ProxyReplace<Counter, CounterBaseLimitedCounter>();
        });

        CheckReplacedSucceed<Counter>(serviceProvider);
    }

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldReplaceTypeInterfaceSuccess(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(ICounter), typeof(Counter), serviceLifetime));
            services.ProxyReplace<ICounter, LimitedCounter>();
        });

        CheckReplacedSucceed<ICounter>(serviceProvider);
    }

    #endregion Type

    #region Factory

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldReplaceFactoryClassSuccess(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(Counter), _ => new Counter(), serviceLifetime));
            services.ProxyReplace<Counter, CounterBaseLimitedCounter>();
        });

        CheckReplacedSucceed<Counter>(serviceProvider);
    }

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldReplaceFactoryInterfaceSuccess(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(ICounter), _ => new Counter(), serviceLifetime));
            services.ProxyReplace<ICounter, LimitedCounter>();
        });

        CheckReplacedSucceed<ICounter>(serviceProvider);
    }

    #endregion Factory

    #region Instance

    [TestMethod]
    public void ShouldReplaceInstanceSingletonClassSuccess()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingleton<Counter>(new Counter());
            services.ProxyReplace<Counter, CounterBaseLimitedCounter>();
        });

        CheckReplacedSucceed<Counter>(serviceProvider);
    }

    [TestMethod]
    public void ShouldReplaceInstanceSingletonInterfaceSuccess()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingleton<ICounter>(new Counter());
            services.ProxyReplace<ICounter, LimitedCounter>();
        });

        CheckReplacedSucceed<ICounter>(serviceProvider);
    }

    #endregion Instance

    [TestMethod]
    public void ShouldThrowOnMultipleLifetime()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(ICounter), _ => new Counter(), ServiceLifetime.Scoped));
            services.Add(ServiceDescriptor.Describe(typeof(ICounter), _ => new Counter(), ServiceLifetime.Singleton));
            Assert.ThrowsException<InvalidOperationException>(() => services.ProxyReplace<ICounter, LimitedCounter>());
        });
    }

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldThrowOnMultipleReplace(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(ICounter), typeof(Counter), serviceLifetime));
            services.ProxyReplace<ICounter, LimitedCounter>();
            Assert.ThrowsException<InvalidOperationException>(() => services.ProxyReplace<ICounter, LimitedCounter>());
        });
    }

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldThrowOnSelfTypeService(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(Counter), typeof(Counter), serviceLifetime));
            Assert.ThrowsException<InvalidOperationException>(() => services.ProxyReplace<Counter, CounterBaseLimitedCounter>());
        });
    }

    [TestMethod]
    public void ShouldThrowOnServiceNotFound()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            Assert.ThrowsException<InvalidOperationException>(() => services.ProxyReplace<ICounter, LimitedCounter>());
        });
    }

    #endregion Public 方法

    #region Protected 方法

    protected static IServiceProvider BuildServiceProvider(Action<IServiceCollection> serviceSetup)
    {
        var services = new ServiceCollection();
        serviceSetup(services);
        return services.BuildServiceProvider();
    }

    protected static void CheckReplacedSucceed<TCounter>(IServiceProvider serviceProvider)
        where TCounter : ICounter
    {
        var counter = serviceProvider.GetRequiredService<TCounter>();

        Assert.AreEqual(0, counter.Count);
        Assert.IsTrue(LimitedCounter.MaxValue > 0);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedCounter.MaxValue, counter.Count);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedCounter.MaxValue, counter.Count);

        using var scope = serviceProvider.CreateScope();
        counter = scope.ServiceProvider.GetRequiredService<TCounter>();

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedCounter.MaxValue, counter.Count);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedCounter.MaxValue, counter.Count);
    }

    #endregion Protected 方法
}
