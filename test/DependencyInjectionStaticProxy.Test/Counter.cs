using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionStaticProxy.Test;

public interface ICounter
{
    #region Public 属性

    public int Count { get; }

    #endregion Public 属性

    #region Public 方法

    public void Increment();

    #endregion Public 方法
}

public class Counter : ICounter
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

public class DeriveCounter : Counter
{ }

public class LimitedCounter : ICounter
{
    #region Public 字段

    public const int MaxValue = 2;

    #endregion Public 字段

    #region Private 字段

    private readonly ICounter _innerCounter;

    #endregion Private 字段

    #region Public 属性

    public int Count => _innerCounter.Count;

    #endregion Public 属性

    #region Public 构造函数

    public LimitedCounter(IReplacedServiceAccessor<ICounter> replacedServiceAccessor)
    {
        _innerCounter = replacedServiceAccessor.Service;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Increment()
    {
        if (_innerCounter.Count >= MaxValue)
        {
            return;
        }
        _innerCounter.Increment();
    }

    #endregion Public 方法
}

public class CounterBaseLimitedCounter : Counter
{
    #region Public 字段

    public const int MaxValue = LimitedCounter.MaxValue;

    #endregion Public 字段

    #region Private 字段

    private readonly ICounter _innerCounter;

    #endregion Private 字段

    #region Public 属性

    public override int Count => _innerCounter.Count;

    #endregion Public 属性

    #region Public 构造函数

    public CounterBaseLimitedCounter(IReplacedServiceAccessor<Counter> replacedServiceAccessor)
    {
        _innerCounter = replacedServiceAccessor.Service;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void Increment()
    {
        if (_innerCounter.Count >= MaxValue)
        {
            return;
        }
        _innerCounter.Increment();
    }

    #endregion Public 方法
}
