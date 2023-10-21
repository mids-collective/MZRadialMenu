using System.Reflection;

namespace Plugin.Attributes;

public class HiddenAttribute : Attribute { }
public class RenameAttribute : Attribute
{
    public string OldName;
    public RenameAttribute(string OldName)
    {
        this.OldName = OldName;
    }
}
public class DisplayNameAttribute : Attribute
{
    public string Name;
    public DisplayNameAttribute(string Name)
    {
        this.Name = Name;
    }
}
public static class Registry
{
    public static List<Type> GetTypes<T>()
    {
        return Assembly.GetCallingAssembly().GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(T))).Where(x => x.IsClass && !x.IsInterface && !x.IsAbstract).ToList();
    }
    public static Dictionary<string, string> TypeRenames()
    {
        var types = Assembly.GetCallingAssembly().GetExportedTypes().Where(x => x.GetCustomAttributes<RenameAttribute>().Count() > 0);
        var ret = new Dictionary<string, string>();
        foreach (var typ in types)
        {
            var attrs = typ.GetCustomAttributes<RenameAttribute>();
            foreach (var atr in attrs)
            {
                ret.Add(typ.FullName!.Replace(typ.Name, atr.OldName), typ.FullName);
            }
        }
        return ret;
    }
}
