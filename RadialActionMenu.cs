using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Dalamud;
using Dalamud.Plugin;
using Dalamud.Utility.Signatures;
using Dalamud.Hooking;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

using MZRadialMenu.Structures;
using MZRadialMenu.Attributes;
using MZRadialMenu.Config;
using MZRadialMenu.Extensions;

using Newtonsoft.Json;
using ImComponents;
using ImGuiNET;



namespace MZRadialMenu;

public unsafe class MZRadialMenu : IDalamudPlugin
{
    public static MZRadialMenu? Instance;
    public string Name => "MZRadialMenu";
    private Wheels ActiveConfig;
    private Wheels ConfigWindow;
    private PluginCommandManager<MZRadialMenu> commandManager;
    private bool ConfigOpen = false;
    private uint retryItem = 0;
    private Dictionary<uint, string>? usables = null;
    //Agents
    private nint itemContextMenuAgent;
    private AgentModule* agentModule;
    private UIModule* uiModule;
    private bool IsGameTextInputActive => uiModule->GetRaptureAtkModule()->AtkModule.IsTextInputActive();
    private nint GetAgentByInternalID(AgentId id) => (nint)agentModule->GetAgentByInternalId(id);
    // Macro Execution
    public delegate void ExecuteMacroDelegate(RaptureShellModule* raptureShellModule, nint macro);
    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 4D 28")]
    public static Hook<ExecuteMacroDelegate>? ExecuteMacroHook;
    public static RaptureShellModule* raptureShellModule;
    public static RaptureMacroModule* raptureMacroModule;
    //Extended Macro Execution
    private static nint numCopiedMacroLinesPtr = nint.Zero;
    public static byte NumCopiedMacroLines
    {
        get => *(byte*)numCopiedMacroLinesPtr;
        set
        {
            if (numCopiedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numCopiedMacroLinesPtr, new[] { value });
        }
    }

    private static nint numExecutedMacroLinesPtr = nint.Zero;
    public static byte NumExecutedMacroLines
    {
        get => *(byte*)numExecutedMacroLinesPtr;
        set
        {
            if (numExecutedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numExecutedMacroLinesPtr, new[] { value });
        }
    }
    //Use Item
    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 7C 24 38")]
    private static delegate* unmanaged<nint, uint, uint, uint, short, void> useItem;
    [Signature("E8 ?? ?? ?? ?? 44 8B 4B 2C")]
    private static delegate* unmanaged<uint, uint, uint> getActionID;
    private const int aetherCompassID = 2_001_886;
    // Command Execution
    public delegate void ProcessChatBoxDelegate(UIModule* uiModule, nint message, nint unused, byte a4);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9")]
    public static ProcessChatBoxDelegate? ProcessChatBox;
    private AdvRadialMenu MyRadialMenu;
    private void RenderWheel()
    {
        for (int i = 0; i < ActiveConfig!.WheelSet.Count; i++)
        {
            var Config = ActiveConfig.WheelSet[i];
            if (Config.key.key != 0x0)
            {
                var open = Dalamud.Keys[Config.key.key];
                if (open && !ActiveConfig.WheelSet.Any(x => x.IsOpen) && uiModule != null && !IsGameTextInputActive)
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
            for (int c = 0; c < ConfigWindow!.WheelSet.Count; c++)
            {
                var Config = ConfigWindow.WheelSet[c];
                ImGui.PushID(c);
                if (ImGui.Button("X"))
                {
                    ConfigWindow.WheelSet.RemoveAt(c);
                }
                else
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Export Wheel"))
                    {
                        var json = JsonConvert.SerializeObject(Config);
                        var exp = $"MZRW_({Convert.ToBase64String(Encoding.UTF8.GetBytes(json))})";
                        ImGui.SetClipboardText(exp);
                    }
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
                ConfigWindow.WheelSet.Add(new Wheel());
            }
            ImGui.SameLine();
            if (ImGui.Button("Import Wheel"))
            {
                var clip = ImGui.GetClipboardText();
                if (clip.StartsWith("MZRW_("))
                {
                    clip = clip[6..^1];
                    var obj = JsonConvert.DeserializeObject<Wheel>(Encoding.UTF8.GetString(Convert.FromBase64String(clip)))!;
                    ConfigWindow.WheelSet.Add(obj);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Save and Close"))
            {
                ConfigOpen = false;
                ActiveConfig = ConfigWindow.DeepCopy();
            }
            ImGui.SameLine();
            if (ImGui.Button("Revert"))
            {
                Dalamud.PluginInterface.SavePluginConfig(ActiveConfig);
                ConfigWindow = ActiveConfig.DeepCopy();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ConfigOpen = false;
                ConfigWindow = ActiveConfig.DeepCopy();
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
        Dalamud.Initialize(dpi);
        MyRadialMenu = new();
        Instance = this;
        commandManager = new PluginCommandManager<MZRadialMenu>(this);
        ActiveConfig = (Wheels?)Dalamud.PluginInterface.GetPluginConfig() ?? new();
        ConfigWindow = ActiveConfig.DeepCopy();
        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        if (Dalamud.ClientState.IsLoggedIn)
        {
            InitCommands();
            InitUsables();
        }
        Dalamud.ClientState.Login += InitCommands;
        Dalamud.ClientState.Login += InitUsables;
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
        if (id == 0 || !usables!.ContainsKey(id is >= 1_000_000 and < 2_000_000 ? id - 1_000_000 : id)) return;

        // Aether Compass
        if (id == aetherCompassID)
        {
            ActionManager.Instance()->UseAction(ActionType.Action, 26988);
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
        if (string.IsNullOrWhiteSpace(name)) return;
        if (usables == null) InitUsables();

        var newName = name.Replace("\uE03C", ""); // Remove HQ Symbol
        var useHQ = newName != name;
        newName = newName.ToLower().Trim(' ');

        UseItem(usables!.First(i => i.Value == newName).Key + (uint)(useHQ ? 1_000_000 : 0));
    }

    private void InitUsables()
    {
        usables = Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.Where(i => i.ItemAction.Row > 0).ToDictionary(i => i.RowId, i => i.Name.ToString().ToLower())
            .Concat(Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.EventItem>()!.Where(i => i.Action.Row > 0).ToDictionary(i => i.RowId, i => i.Name.ToString().ToLower()))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        usables[aetherCompassID] = Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.EventItem>()!.GetRow(aetherCompassID)?.Name.ToString().ToLower()!;
    }

    private void InitCommands()
    {
        Dalamud.GameInteropProvider.InitializeFromAttributes(this);

        uiModule = Framework.Instance()->GetUiModule();
        agentModule = uiModule->GetAgentModule();
        raptureShellModule = uiModule->GetRaptureShellModule();
        raptureMacroModule = uiModule->GetRaptureMacroModule();
        numCopiedMacroLinesPtr = Dalamud.SigScanner.ScanText("49 8D 5E 70 BF ?? 00 00 00") + 0x5;
        numExecutedMacroLinesPtr = Dalamud.SigScanner.ScanText("41 83 F8 ?? 0F 8D ?? ?? ?? ?? 49 6B C8 68") + 0x3;

        itemContextMenuAgent = GetAgentByInternalID(AgentId.InventoryContext);

        ExecuteMacroHook!.Enable();

        Dalamud.PluginLog.Info("Commands Initialized");
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
                        ExecuteMacroHook!.Original(raptureShellModule, (nint)raptureMacroModule + 0x58 + (Macro.size * val));
                    }
                    break;
            }
            return;
        }
        var stringPtr = nint.Zero;
        try
        {
            stringPtr = Marshal.AllocHGlobal(UTF8String.size);
            using var str = new UTF8String(stringPtr, command);
            Marshal.StructureToPtr(str, stringPtr, false);
            ProcessChatBox!(uiModule, stringPtr, nint.Zero, 0);
        }
        catch
        {
            Dalamud.PluginLog.Error("Error with injecting command");
        }
        Marshal.FreeHGlobal(stringPtr);
    }

    public void Dispose()
    {
        NumCopiedMacroLines = 15;
        NumExecutedMacroLines = 15;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
        Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
        Dalamud.ClientState.Login -= InitCommands;
        Dalamud.ClientState.Login -= InitUsables;
        ExecuteMacroHook!.Dispose();
        commandManager.Dispose();
    }

    public static void ExecuteMacroDetour(RaptureShellModule* raptureShellModule, nint macro)
    {
        NumCopiedMacroLines = Macro.numLines;
        NumExecutedMacroLines = Macro.numLines;
        ExecuteMacroHook!.Original(raptureShellModule, macro);
    }
}