using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using MZRadialMenu.Config;

namespace MZRadialMenu;
public class Wheels : IPluginConfiguration
{
    public List<Wheel> WheelSet = new();
    public int Version { get; set; } = 2;
}