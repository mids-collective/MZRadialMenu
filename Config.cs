using System.Collections.Generic;
using Dalamud.Configuration;
using MZRadialMenu.Config;

namespace MZRadialMenu
{
    public class Wheels : IPluginConfiguration
    {
        public List<Wheel> WheelSet = new();
        public string UUID = System.Guid.NewGuid().ToString();
        public int Version { get; set; } = 2;
    }
}