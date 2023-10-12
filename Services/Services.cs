namespace MZRadialMenu.Services;

public static class Service<T> where T : IDisposable
{
    private static Lazy<T> instance = new Lazy<T>(() => (T)Activator.CreateInstance(typeof(T), true)!);
    public static T Instance { get { return instance.Value; } }
}