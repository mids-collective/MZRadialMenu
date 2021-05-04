using System.Collections.Generic;
using Dalamud.Configuration;

namespace MZRadialMenu
{
    public enum ST
    {
        Menu, Shortcut,
    }
    public struct Shortcut
    {
        public string Title;
        public ST Type;
        public List<Shortcut> sublist;
        public string Command;
    }
    public class Wheel : IPluginConfiguration
    {
        public int key = 0x58;
        public List<Shortcut> RootMenu = new();

        public int Version { get; set; } = 1;
    }
}