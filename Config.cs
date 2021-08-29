using System.Collections.Generic;
using Dalamud.Configuration;
using Newtonsoft.Json;
namespace MZRadialMenu
{
    public enum ST
    {
        Menu, Shortcut
    }
    public class Shortcut
    {
        public string Title = string.Empty;
        public ST Type = ST.Shortcut;
        public List<Shortcut> sublist = new();
        public System.Guid UUID = System.Guid.NewGuid();
        public string Command = string.Empty;
    }
    public class Wheels : IPluginConfiguration
    {
        public List<Wheel> WheelSet = new();
        public System.Guid UUID = System.Guid.NewGuid();
        public int Version { get; set; } = 2;
    }
    public class Wheel
    {
        public HotkeyButton key = new();
        public string Name = string.Empty;
        [JsonIgnore]
        public bool IsOpen = false;
        public System.Guid UUID = System.Guid.NewGuid();
        public List<Shortcut> RootMenu = new();
    }
}