using Microsoft.Extensions.Primitives;

namespace WebApplicationSample;

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
