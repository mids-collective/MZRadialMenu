using Dalamud.Configuration;
using MZRadialMenu.Config;

namespace Plugin;
public class ConfigFile : IPluginConfiguration
{
    public List<Wheel> WheelSet = new();
    public HashSet<string> AssemblyLocations = new();
    public int Version { get; set; } = 2;
}