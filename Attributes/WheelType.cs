using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Plugin.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class HiddenAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public class RenameAttribute(string OldName) : Attribute
{
    public string OldName = OldName;
}

[AttributeUsage(AttributeTargets.Class)]
public class DisplayNameAttribute(string Name) : Attribute
{
    public string Name = Name;
}
public static class Registry
{
    public static List<Type> GetTypes<T>()
    {
        return Assembly.GetCallingAssembly().GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(T))).Where(x => x.IsClass && !x.IsInterface && !x.IsAbstract).ToList();
    }
    public static Dictionary<string, string> TypeRenames()
    {
        var types = Assembly.GetCallingAssembly().GetExportedTypes().Where(x => x.GetCustomAttributes<RenameAttribute>().Any());
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
