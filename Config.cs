using Dalamud.Configuration;
using MZRadialMenu.Config;

namespace MZRadialMenu;
public class ConfigFile : IPluginConfiguration
{
    public List<Wheel> WheelSet = new();
    public int Version { get; set; } = 2;
}