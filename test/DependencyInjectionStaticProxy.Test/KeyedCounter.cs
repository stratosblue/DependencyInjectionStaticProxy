using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionStaticProxy.Test;

public interface IKeyedCounter
{
    #region Public 属性

    public int Count { get; }

    #endregion Public 属性

    #region Public 方法

    public void Increment();

    #endregion Public 方法
}

public class KeyedCounter : IKeyedCounter
{
    #region Public 属性

    public virtual int Count { get; private set; }

    #endregion Public 属性

    #region Public 方法

    public virtual void Increment()
    {
        Count++;
    }

    #endregion Public 方法
}

public class DeriveKeyedCounter : KeyedCounter
{ }

public class LimitedKeyedCounter : IKeyedCounter
{
    #region Public 字段

    public const int MaxValue = 2;

    #endregion Public 字段

    #region Private 字段

    private readonly IKeyedCounter _innerKeyedCounter;

    #endregion Private 字段

    #region Public 属性

    public int Count => _innerKeyedCounter.Count;

    #endregion Public 属性

    #region Public 构造函数

    public LimitedKeyedCounter([FromKeyedServices(GenericKeyedServiceTest.ServiceName1)] IReplacedServiceAccessor<IKeyedCounter> replacedServiceAccessor)
    {
        _innerKeyedCounter = replacedServiceAccessor.Service;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Increment()
    {
        if (_innerKeyedCounter.Count >= MaxValue)
        {
            return;
        }
        _innerKeyedCounter.Increment();
    }

    #endregion Public 方法
}

public class KeyedCounterBaseLimitedKeyedCounter : KeyedCounter
{
    #region Public 字段

    public const int MaxValue = LimitedKeyedCounter.MaxValue;

    #endregion Public 字段

    #region Private 字段

    private readonly IKeyedCounter _innerKeyedCounter;

    #endregion Private 字段

    #region Public 属性

    public override int Count => _innerKeyedCounter.Count;

    #endregion Public 属性

    #region Public 构造函数

    public KeyedCounterBaseLimitedKeyedCounter([FromKeyedServices(GenericKeyedServiceTest.ServiceName1)] IReplacedServiceAccessor<KeyedCounter> replacedServiceAccessor)
    {
        _innerKeyedCounter = replacedServiceAccessor.Service;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void Increment()
    {
        if (_innerKeyedCounter.Count >= MaxValue)
        {
            return;
        }
        _innerKeyedCounter.Increment();
    }

    #endregion Public 方法
}
