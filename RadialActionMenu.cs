using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

using Dalamud;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility.Signatures;
using Dalamud.Hooking;


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
        //Agents
        private IntPtr itemContextMenuAgent;
        private AgentModule* agentModule;
        private UIModule* uiModule;
        private bool IsGameTextInputActive => uiModule->GetRaptureAtkModule()->AtkModule.IsTextInputActive() != 0;
        private IntPtr GetAgentByInternalID(AgentId id) => (IntPtr)agentModule->GetAgentByInternalId(id);
        // Macro Execution
        public delegate void ExecuteMacroDelegate(RaptureShellModule* raptureShellModule, IntPtr macro);
        [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 4D 28")]
        public ExecuteMacroDelegate ExecuteMacro;
        public static RaptureShellModule* raptureShellModule;
        public static RaptureMacroModule* raptureMacroModule;
        //Extended Macro Execution
        private static IntPtr numCopiedMacroLinesPtr = IntPtr.Zero;
        public static byte NumCopiedMacroLines
        {
            get => *(byte*)numCopiedMacroLinesPtr;
            set
            {
                if (numCopiedMacroLinesPtr != IntPtr.Zero)
                    SafeMemory.WriteBytes(numCopiedMacroLinesPtr, new[] { value });
            }
        }

        private static IntPtr numExecutedMacroLinesPtr = IntPtr.Zero;
        public static byte NumExecutedMacroLines
        {
            get => *(byte*)numExecutedMacroLinesPtr;
            set
            {
                if (numExecutedMacroLinesPtr != IntPtr.Zero)
                    SafeMemory.WriteBytes(numExecutedMacroLinesPtr, new[] { value });
            }
        }
        // Use Items
        [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 41 B0 01 BA 13 00 00 00")]
        private delegate* unmanaged<IntPtr, uint, uint, uint, short, void> useItem;
        [Signature("E8 ?? ?? ?? ?? 44 8B 4B 2C")]
        private delegate* unmanaged<uint, uint, uint> getActionID;
        private const int aetherCompassID = 2_001_886;
        // Command Execution
        // Command Execution
        private delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);
        [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9")]
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
                    if (open && !Configuration.WheelSet.Any(x => x.IsOpen) && uiModule != null && !IsGameTextInputActive)
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
        }
        public MZRadialMenu(DalamudPluginInterface dpi)
        {
            MyRadialMenu = new();
            Instance = this;
            Dalamud.Initialize(dpi);
            commandManager = new PluginCommandManager<MZRadialMenu>(this);
            Configuration = (Wheels)Dalamud.PluginInterface.GetPluginConfig() ?? new();
            Dalamud.PluginInterface.UiBuilder.Draw += Draw;
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
            if (Dalamud.ClientState.IsLoggedIn)
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
            raptureShellModule = uiModule->GetRaptureShellModule();
            raptureMacroModule = uiModule->GetRaptureMacroModule();
            SignatureHelper.Initialise(this, true);
            try
            {
                numCopiedMacroLinesPtr = Dalamud.SigScanner.ScanText("49 8D 5E 70 BF ?? 00 00 00") + 0x5;
                numExecutedMacroLinesPtr = Dalamud.SigScanner.ScanText("41 83 F8 ?? 0F 8D ?? ?? ?? ?? 49 6B C8 68") + 0x3;
            }
            catch
            {
                PluginLog.LogError("Failed to Load ExecuteMacro");
            }
            try
            {
                itemContextMenuAgent = GetAgentByInternalID(AgentId.InventoryContext);

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
                            ExecuteMacro(raptureShellModule, (IntPtr)raptureMacroModule + 0x58 + (Macro.size * val));
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