using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionStaticProxy.Test;

[TestClass]
public class GenericKeyedServiceTest
{
    #region Public 字段

    public const string ServiceName1 = "Service1";

    public const string ServiceName2 = "Service2";

    #endregion Public 字段

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
            services.Add(ServiceDescriptor.Describe(typeof(KeyedCounter), typeof(DeriveKeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(KeyedCounter), ServiceName1, typeof(DeriveKeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(KeyedCounter), ServiceName2, typeof(DeriveKeyedCounter), serviceLifetime));
            services.ProxyReplaceKeyed<KeyedCounter, KeyedCounterBaseLimitedKeyedCounter>(ServiceName1);
        });

        CheckReplacedSucceed<KeyedCounter>(serviceProvider);
    }

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldReplaceTypeInterfaceSuccess(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(IKeyedCounter), typeof(KeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName1, typeof(KeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName2, typeof(KeyedCounter), serviceLifetime));
            services.ProxyReplaceKeyed<IKeyedCounter, LimitedKeyedCounter>(ServiceName1);
        });

        CheckReplacedSucceed<IKeyedCounter>(serviceProvider);
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
            services.Add(ServiceDescriptor.Describe(typeof(KeyedCounter), _ => new KeyedCounter(), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(KeyedCounter), ServiceName1, (_, _) => new KeyedCounter(), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(KeyedCounter), ServiceName2, (_, _) => new KeyedCounter(), serviceLifetime));
            services.ProxyReplaceKeyed<KeyedCounter, KeyedCounterBaseLimitedKeyedCounter>(ServiceName1);
        });

        CheckReplacedSucceed<KeyedCounter>(serviceProvider);
    }

    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Transient)]
    [TestMethod]
    public void ShouldReplaceFactoryInterfaceSuccess(ServiceLifetime serviceLifetime)
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(IKeyedCounter), _ => new KeyedCounter(), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName1, (_, _) => new KeyedCounter(), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName2, (_, _) => new KeyedCounter(), serviceLifetime));
            services.ProxyReplaceKeyed<IKeyedCounter, LimitedKeyedCounter>(ServiceName1);
        });

        CheckReplacedSucceed<IKeyedCounter>(serviceProvider);
    }

    #endregion Factory

    #region Instance

    [TestMethod]
    public void ShouldReplaceInstanceSingletonClassSuccess()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingleton<KeyedCounter>(new KeyedCounter());
            services.AddKeyedSingleton<KeyedCounter>(ServiceName1, new KeyedCounter());
            services.AddKeyedSingleton<KeyedCounter>(ServiceName2, new KeyedCounter());
            services.ProxyReplaceKeyed<KeyedCounter, KeyedCounterBaseLimitedKeyedCounter>(ServiceName1);
        });

        CheckReplacedSucceed<KeyedCounter>(serviceProvider);
    }

    [TestMethod]
    public void ShouldReplaceInstanceSingletonInterfaceSuccess()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingleton<IKeyedCounter>(new KeyedCounter());
            services.AddKeyedSingleton<IKeyedCounter>(ServiceName1, new KeyedCounter());
            services.AddKeyedSingleton<IKeyedCounter>(ServiceName2, new KeyedCounter());
            services.ProxyReplaceKeyed<IKeyedCounter, LimitedKeyedCounter>(ServiceName1);
        });

        CheckReplacedSucceed<IKeyedCounter>(serviceProvider);
    }

    #endregion Instance

    [TestMethod]
    public void ShouldThrowOnMultipleLifetime()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(IKeyedCounter), _ => new KeyedCounter(), ServiceLifetime.Scoped));
            services.Add(ServiceDescriptor.Describe(typeof(IKeyedCounter), _ => new KeyedCounter(), ServiceLifetime.Singleton));

            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName1, (_, _) => new KeyedCounter(), ServiceLifetime.Scoped));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName1, (_, _) => new KeyedCounter(), ServiceLifetime.Singleton));

            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName2, (_, _) => new KeyedCounter(), ServiceLifetime.Scoped));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName2, (_, _) => new KeyedCounter(), ServiceLifetime.Singleton));

            Assert.ThrowsExactly<InvalidOperationException>(() => services.ProxyReplaceKeyed<IKeyedCounter, LimitedKeyedCounter>(ServiceName1));
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
            services.Add(ServiceDescriptor.Describe(typeof(IKeyedCounter), typeof(KeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName1, typeof(KeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(IKeyedCounter), ServiceName2, typeof(KeyedCounter), serviceLifetime));

            services.ProxyReplaceKeyed<IKeyedCounter, LimitedKeyedCounter>(ServiceName1);
            Assert.ThrowsExactly<InvalidOperationException>(() => services.ProxyReplaceKeyed<IKeyedCounter, LimitedKeyedCounter>(ServiceName1));
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
            services.Add(ServiceDescriptor.Describe(typeof(KeyedCounter), typeof(KeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(KeyedCounter), ServiceName1, typeof(KeyedCounter), serviceLifetime));
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(KeyedCounter), ServiceName2, typeof(KeyedCounter), serviceLifetime));
            Assert.ThrowsExactly<InvalidOperationException>(() => services.ProxyReplaceKeyed<KeyedCounter, KeyedCounterBaseLimitedKeyedCounter>(ServiceName1));
        });
    }

    [TestMethod]
    public void ShouldThrowOnServiceNotFound()
    {
        var serviceProvider = BuildServiceProvider(services =>
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => services.ProxyReplaceKeyed<IKeyedCounter, LimitedKeyedCounter>(ServiceName1));
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

    protected static void CheckReplacedSucceed<TKeyedCounter>(IServiceProvider serviceProvider)
        where TKeyedCounter : IKeyedCounter
    {
        var counter = serviceProvider.GetRequiredKeyedService<TKeyedCounter>(ServiceName1);

        Assert.AreEqual(0, counter.Count);
        Assert.IsGreaterThan(0, LimitedKeyedCounter.MaxValue);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedKeyedCounter.MaxValue, counter.Count);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedKeyedCounter.MaxValue, counter.Count);

        using var scope = serviceProvider.CreateScope();
        counter = scope.ServiceProvider.GetRequiredKeyedService<TKeyedCounter>(ServiceName1);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedKeyedCounter.MaxValue, counter.Count);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(LimitedKeyedCounter.MaxValue, counter.Count);

        //--------------------------------------------------

        counter = serviceProvider.GetRequiredKeyedService<TKeyedCounter>(ServiceName2);

        Assert.AreEqual(0, counter.Count);
        Assert.IsGreaterThan(0, LimitedKeyedCounter.MaxValue);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(2, counter.Count);

        counter.Increment();
        counter.Increment();

        Assert.AreEqual(4, counter.Count);
    }

    #endregion Protected 方法
}
