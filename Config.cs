using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using MZRadialMenu.Config;

namespace MZRadialMenu;
public class Wheels : IPluginConfiguration
{
    public List<Wheel> WheelSet = new();
    [JsonIgnore]
    public string UUID = System.Guid.NewGuid().ToString();
    public int Version { get; set; } = 2;
}