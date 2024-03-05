# DependencyInjectionStaticProxy

Service static proxy tool for DI container, Create a static proxy for the service without changing the service implementation within the 'DI' container;

`DI` 容器的服务静态代理工具，在不更改 `DI` 容器内服务实现的前提下为服务创建静态代理；

 - 使用 `IReplacedServiceAccessor<TService>` 访问容器内的原始实例；

-------

### 使用示例
静态代理容器内的 `IConfiguration` 而不用关心其具体实现/不改变其原本逻辑

#### 创建静态代理类
代理 `IConfiguration` 并在key被访问时输出到控制台
```C#
public class ProxiedConfiguration : IConfiguration
{
    private readonly IConfiguration _configuration;

    public ProxiedConfiguration(IReplacedServiceAccessor<IConfiguration> replacedServiceAccessor)
    {
        _configuration = replacedServiceAccessor.Service;
    }

    public string this[string key]
    {
        get => _configuration[AccessLog(key)];
        set => _configuration[AccessLog(key)] = value;
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        return _configuration.GetChildren();
    }

    public IChangeToken GetReloadToken()
    {
        return _configuration.GetReloadToken();
    }

    public IConfigurationSection GetSection(string key)
    {
        return _configuration.GetSection(AccessLog(key));
    }

    public string AccessLog(string key)
    {
        Console.WriteLine($"\"{key}\" has been accessed.");
        return key;
    }
}
```

#### 代理并替换服务容器内的 `IConfiguration`
```C#
services.ProxyReplace<IConfiguration, ProxiedConfiguration>();
```
