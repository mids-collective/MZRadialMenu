using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace MZRadialMenu.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class WheelTypeAttribute : Attribute
{
    public bool Hide;
    public string Name;
    public WheelTypeAttribute(string name = "", bool Hidden = false)
    {
        Name = name;
        Hide = Hidden;
    }
}
public static class Registry
{
    public static List<Type> GetTypes<T>() where T : Attribute
    {
        return typeof(T).Assembly.GetTypes().Where(attr => attr.GetCustomAttributes<T>(false).ToArray().Length > 0).ToList();
    }
    public static List<T> GetAttributes<T>() where T : Attribute
    {
        return GetTypes<T>().SelectMany(att => att.GetCustomAttributes<T>()).Select(at => (T)at).ToList();
    }
}
