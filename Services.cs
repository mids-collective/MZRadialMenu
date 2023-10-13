using System.Reflection;

namespace Plugin;

public static class Service<T> where T : IService<T>
{
    private static Lazy<T> instance = new Lazy<T>(() => (T)Activator.CreateInstance(typeof(T), true)!);
    public static T Instance { get { return instance.Value; } }
}

public interface IService<T> : IDisposable where T : IService<T>
{
    public abstract static T Instance { get; }
}

public class ServiceInitializer
{
    private List<IDisposable> services;
    public ServiceInitializer()
    {
        services = Assembly.GetCallingAssembly().GetTypes().Where(x => x.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>))).Select(x => x.GetProperty("Instance")!.GetValue(null)).Cast<IDisposable>().ToList();
        DalamudApi.PluginLog.Info($"Initialized {services.Count} Services");
    }
    public void Dispose()
    {
        foreach (var srv in services)
        {
            srv.Dispose();
        }
    }
}