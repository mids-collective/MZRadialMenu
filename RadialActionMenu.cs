using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using MZRadialMenu.Structures;
using MZRadialMenu.Attributes;
using MZRadialMenu.Config;
using Dalamud.Logging;
using Dalamud.Plugin;
using Newtonsoft.Json;
using ImComponents;
using ImGuiNET;
namespace MZRadialMenu
{
    public class MZRadialMenu : IDalamudPlugin
    {
        public static MZRadialMenu Instance;
        public string Name => "MZRadialMenu";
        private Wheels Configuration;
        private PluginCommandManager<MZRadialMenu> commandManager;
        private bool ConfigOpen = false;
        private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
        private ProcessChatBoxDelegate ProcessChatBox;
        private IntPtr uiModule = IntPtr.Zero;
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
                    //File.WriteAllText(Dalamud.PluginInterface.ConfigFile.FullName,
                    //    JsonConvert.SerializeObject(Configuration, new Config.Converter()));
                    Dalamud.PluginInterface.SavePluginConfig(Configuration);
                }
                ImGui.SameLine();
                if (ImGui.Button("Save and Close"))
                {
                    //File.WriteAllText(Dalamud.PluginInterface.ConfigFile.FullName,
                    //    JsonConvert.SerializeObject(Configuration, new Config.Converter()));
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
        private void InitCommands()
        {
            try
            {
                ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(Dalamud.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));
                uiModule = Dalamud.GameGui.GetUIModule();
                if (uiModule != IntPtr.Zero && ProcessChatBox != null)
                    PluginLog.Log("Commands Initialized!");
                else
                    PluginLog.Log("Commands Initialization Failed!");
            }
            catch
            {
                PluginLog.Log("Error with loading signatures");
            }
        }

        public void ExecuteCommand(string command)
        {
            if (uiModule == IntPtr.Zero || ProcessChatBox == null)
            {
                InitCommands();
            }
            var stringPtr = IntPtr.Zero;
            try
            {
                PluginLog.Log($"Executing command {command}");
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
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
            Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
            Dalamud.ClientState.Login -= handleLogin;
            commandManager.Dispose();
        }
    }
}