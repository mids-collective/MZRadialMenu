
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.Game.Network;
using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace MZRadialMenu
{
    public class Dalamud
    {
        public static void Initialize(DalamudPluginInterface pluginInterface)
            => pluginInterface.Create<Dalamud>();

        // @formatter:off
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null;
        [PluginService] public static CommandManager Commands { get; private set; } = null;
        [PluginService] public static SigScanner SigScanner { get; private set; } = null;
        [PluginService] public static DataManager GameData { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null;
        [PluginService] public static ChatGui                Chat            { get; private set; } = null!;
        //[PluginService] public static ChatHandlers           ChatHandlers    { get; private set; } = null!;
        //[PluginService] public static Framework Framework { get; private set; } = null;
        //[PluginService] public static GameNetwork            Network         { get; private set; } = null!;
        //[PluginService] public static Condition              Conditions      { get; private set; } = null!;
        [PluginService] public static KeyState Keys { get; private set; } = null;
        [PluginService] public static GameGui GameGui { get; private set; } = null;
        //[PluginService] public static FlyTextGui             FlyTexts        { get; private set; } = null!;
        //[PluginService] public static ToastGui               Toasts          { get; private set; } = null!;
        //[PluginService] public static JobGauges              Gauges          { get; private set; } = null!;
        //[PluginService] public static PartyFinderGui         PartyFinder     { get; private set; } = null!;
        //[PluginService] public static BuddyList              Buddies         { get; private set; } = null!;
        //[PluginService] public static PartyList              Party           { get; private set; } = null!;
        //[PluginService] public static TargetManager Targets { get; private set; } = null!;
        //[PluginService] public static ObjectTable Objects { get; private set; } = null!;
        //[PluginService] public static FateTable              Fates           { get; private set; } = null!;
        //[PluginService] public static LibcFunction           LibC            { get; private set; } = null!;
        [PluginService] public static AetheryteList           AetheryteList            { get; private set; } = null!;
        // @formatter:on
    }
    
}