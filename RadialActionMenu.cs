using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

using Dalamud;
using Dalamud.Logging;
using Dalamud.Plugin;

using ImComponents;
using ImGuiNET;

using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

using MZRadialMenu.Structures;
using MZRadialMenu.Attributes;
using MZRadialMenu.Config;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace MZRadialMenu
{
    public unsafe class MZRadialMenu : IDalamudPlugin
    {
        public static MZRadialMenu Instance;
        public string Name => "MZRadialMenu";
        private Wheels Configuration;
        private PluginCommandManager<MZRadialMenu> commandManager;
        private bool ConfigOpen = false;
        private uint retryItem = 0;
        private Dictionary<uint, string> usables;
        public IntPtr itemContextMenuAgent;
        //Agents
        public AgentModule* agentModule;
        public RaptureShellModule* shellModule;
        public RaptureMacroModule* macroModule;
        private UIModule* uiModule;
        public IntPtr GetAgentByInternalID(uint id) => (IntPtr)agentModule->GetAgentByInternalID(id);
        // Macro Execution
        public delegate void ExecuteMacroDelegate(RaptureShellModule* raptureShellModule, IntPtr macro);
        public ExecuteMacroDelegate ExecuteMacro;
        //Extended Macro Execution
        public IntPtr numCopiedMacroLinesPtr = IntPtr.Zero;
        public byte NumCopiedMacroLines
        {
            get => *(byte*)numCopiedMacroLinesPtr;
            set
            {
                if (numCopiedMacroLinesPtr != IntPtr.Zero)
                    SafeMemory.WriteBytes(numCopiedMacroLinesPtr, new[] { value });
            }
        }

        public IntPtr numExecutedMacroLinesPtr = IntPtr.Zero;
        public byte NumExecutedMacroLines
        {
            get => *(byte*)numExecutedMacroLinesPtr;
            set
            {
                if (numExecutedMacroLinesPtr != IntPtr.Zero)
                    SafeMemory.WriteBytes(numExecutedMacroLinesPtr, new[] { value });
            }
        }
        // Use Items
        private delegate* unmanaged<IntPtr, uint, uint, uint, short, void> useItem;
        private delegate* unmanaged<uint, uint, uint> getActionID;
        private const int aetherCompassID = 2_001_886;
        //ChatBox Execution
        private delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);
        private ProcessChatBoxDelegate ProcessChatBox;
        private AdvRadialMenu MyRadialMenu;
        private void RenderWheel()
        {
            for (int i = 0; i < Configuration.WheelSet.Count; i++)
            {
                var Config = Configuration.WheelSet[i];
                if (Config.key.key != 0x0)
                {
                    var open = Dalamud.Keys[Config.key.key];
                    if (open && !Configuration.WheelSet.Any(x => x.IsOpen))
                    {
                        Config.IsOpen = true;
                        ImGui.OpenPopup("##Wheel", ImGuiPopupFlags.NoOpenOverExistingPopup);
                    }
                    Config.Render(MyRadialMenu, open);
                    if (!open)
                    {
                        Config.IsOpen = false;
                    }
                }
            }
        }
        private void ConfigRender()
        {
            if (ConfigOpen)
            {
                ImGui.Begin("MZ Radial Menu Config", ref ConfigOpen);
                for (int c = 0; c < Configuration.WheelSet.Count; c++)
                {
                    var Config = Configuration.WheelSet[c];
                    ImGui.PushID(c);
                    if (ImGui.Button("X"))
                    {
                        Configuration.WheelSet.RemoveAt(c);
                    }
                    else
                    {
                        ImGui.SameLine();
                        ImGui.PushID(Config.UUID.ToString());
                        if (ImGui.TreeNode(Config.UUID.ToString(), Config.Title))
                        {
                            Config.key.Render();
                            Config.RawRender();
                            ImGui.TreePop();
                        }
                        ImGui.PopID();
                    }
                    ImGui.PopID();
                }
                if (ImGui.Button("New Wheel"))
                {
                    Configuration.WheelSet.Add(new Wheel());
                }
                ImGui.SameLine();
                if (ImGui.Button("Save"))
                {
                    Dalamud.PluginInterface.SavePluginConfig(Configuration);
                }
                ImGui.SameLine();
                if (ImGui.Button("Save and Close"))
                {
                    Dalamud.PluginInterface.SavePluginConfig(Configuration);
                    ConfigOpen = false;
                }
                ImGui.End();
            }
        }
        private void Draw()
        {
            ConfigRender();
            RenderWheel();
            //MyRadialMenu.DebugMenu();
        }
        public MZRadialMenu(DalamudPluginInterface dpi)
        {
            MyRadialMenu = new();
            Dalamud.Initialize(dpi);
            commandManager = new PluginCommandManager<MZRadialMenu>(this);
            Instance = this;
            Configuration = (Wheels)Dalamud.PluginInterface.GetPluginConfig() ?? new();
            Dalamud.PluginInterface.UiBuilder.Draw += Draw;
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
            if (dpi.Reason is PluginLoadReason.Reload or PluginLoadReason.Installer or PluginLoadReason.Update)
            {
                InitCommands();
            }
            Dalamud.ClientState.Login += handleLogin;
            

        }
        private void handleLogin(object sender, EventArgs args)
        {
            InitCommands();
        }
        private void ToggleConfig()
        {
            ConfigOpen = !ConfigOpen;
        }
        [Command("/pwheels")]
        [HelpMessage("Show or hide plugin configuation")]
        private void ToggleConfig(string cmd, string args)
        {
            ToggleConfig();
        }
        public void UseItem(uint id)
        {
            if (id == 0 || !usables.ContainsKey(id is >= 1_000_000 and < 2_000_000 ? id - 1_000_000 : id)) return;

            // Aether Compass
            if (id == aetherCompassID)
            {
                ActionManager.Instance()->UseAction(ActionType.Spell, 26988);
                return;
            }

            // Dumb fix for dumb bug
            if (retryItem == 0 && id < 2_000_000)
            {
                var actionID = getActionID(2, id);
                if (actionID == 0)
                {
                    retryItem = id;
                    return;
                }
            }

            useItem(itemContextMenuAgent, id, 9999, 0, 0);
        }
        public void UseItem(string name)
        {
            if (usables == null || string.IsNullOrWhiteSpace(name)) return;

            var newName = name.Replace("\uE03C", ""); // Remove HQ Symbol
            var useHQ = newName != name;
            newName = newName.ToLower().Trim(' ');

            try { UseItem(usables.First(i => i.Value == newName).Key + (uint)(useHQ ? 1_000_000 : 0)); }
            catch { }
        }
        private void InitCommands()
        {
            uiModule = Framework.Instance()->GetUiModule();
            agentModule = uiModule->GetAgentModule();
            shellModule = uiModule->GetRaptureShellModule();
            macroModule = uiModule->GetRaptureMacroModule();
            try
            {
                ExecuteMacro = Marshal.GetDelegateForFunctionPointer<ExecuteMacroDelegate>(Dalamud.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 4D 28"));
                numCopiedMacroLinesPtr = Dalamud.SigScanner.ScanText("49 8D 5E 70 BF ?? 00 00 00") + 0x5;
                numExecutedMacroLinesPtr = Dalamud.SigScanner.ScanText("41 83 F8 ?? 0F 8D ?? ?? ?? ?? 49 6B C8 68") + 0x3;

            }
            catch
            {
                PluginLog.LogError("Failed to Load ExecuteMacro");
            }
            try
            {
                ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(Dalamud.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));
                if (ProcessChatBox != null)
                    PluginLog.Log("Commands Initialized!");
            }
            catch
            {
                PluginLog.LogError("Failed to Load ProcessChatBox");
            }
            try
            {
                getActionID = (delegate* unmanaged<uint, uint, uint>)Dalamud.SigScanner.ScanText("E8 ?? ?? ?? ?? 44 8B 4B 2C");
            }
            catch
            {
                PluginLog.LogError("Failed to Load GetActionID");
            }
            try
            {
                useItem = (delegate* unmanaged<IntPtr, uint, uint, uint, short, void>)Dalamud.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 41 B0 01 BA 13 00 00 00");
                itemContextMenuAgent = GetAgentByInternalID(10);

                usables = Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>().Where(i => i.ItemAction.Row > 0).ToDictionary(i => i.RowId, i => i.Name.ToString().ToLower())
                    .Concat(Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.EventItem>().Where(i => i.Action.Row > 0).ToDictionary(i => i.RowId, i => i.Name.ToString().ToLower()))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                usables[aetherCompassID] = Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.EventItem>()!.GetRow(aetherCompassID)?.Name.ToString().ToLower();
            }
            catch { PluginLog.LogError("Failed to load UseItem"); }
        }

        public void ExecuteCommand(string command)
        {
            if (ProcessChatBox == null)
            {
                InitCommands();
            }
            if (command.StartsWith("//"))
            {
                command = command[2..].ToLower();
                switch (command[0])
                {
                    case 'i':
                        UseItem(command[2..]);
                        break;
                    case 'm':
                        int.TryParse(command[1..], out int val);
                        if (val is >= 0 and < 200)
                        {
                            ExecuteMacro(shellModule, (IntPtr)macroModule + 0x58 + (Macro.size * val));
                        }
                        break;
                }
                return;
            }
            var stringPtr = IntPtr.Zero;
            try
            {
                stringPtr = Marshal.AllocHGlobal(UTF8String.size);
                using var str = new UTF8String(stringPtr, command);
                Marshal.StructureToPtr(str, stringPtr, false);
                ProcessChatBox(uiModule, stringPtr, IntPtr.Zero, 0);
            }
            catch
            {
                PluginLog.Error("Error with injecting command");
            }
            Marshal.FreeHGlobal(stringPtr);
        }

        public void Dispose()
        {
            NumCopiedMacroLines = 15;
            NumExecutedMacroLines = 15;
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
            Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
            Dalamud.ClientState.Login -= handleLogin;
            commandManager.Dispose();
        }
    }
}