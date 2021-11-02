using System;
using System.Runtime.InteropServices;
using System.Linq;
using MZRadialMenu.Structures;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImComponents;
using ImGuiNET;
namespace MZRadialMenu
{
    public class MZRadialMenu : IDalamudPlugin
    {
        public string Name => "MZRadialMenu";
        private Wheels Configuration;
        private PluginCommandManager<MZRadialMenu> commandManager;
        private bool ConfigOpen = false;
        private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
        private ProcessChatBoxDelegate ProcessChatBox;
        private IntPtr uiModule = IntPtr.Zero;
        private AdvRadialMenu MyRadialMenu;
        private void RenderWheel(Shortcut ct)
        {
            if (MyRadialMenu.BeginRadialMenu(ct.Title))
            {
                foreach (var sh in ct.sublist)
                {
                    if (sh.Type == ST.Shortcut)
                    {
                        MyRadialMenu.RadialMenuItem(sh.Title, (n) => ExecuteCommand(sh.Command));
                    }
                    else if (sh.Type == ST.Menu)
                    {
                        RenderWheel(sh);
                    }
                }
                MyRadialMenu.EndRadialMenu();
            }
        }
        private void RenderWheel()
        {
            for (int i = 0; i < Configuration.WheelSet.Count; i++)
            {
                var Config = Configuration.WheelSet[i];
                var ConfigName = $"##Wheel";
                var open = Dalamud.Keys[Config.key.key];
                if (open && !Configuration.WheelSet.Any(x => x.IsOpen))
                {
                    Config.IsOpen = true;
                    ImGui.OpenPopup(ConfigName, ImGuiPopupFlags.NoOpenOverExistingPopup);
                }
                if (Config.IsOpen)
                {
                    if (MyRadialMenu.BeginRadialPopup(ConfigName, open))
                    {
                        foreach (var sh in Config.RootMenu)
                        {
                            if (sh.Type == ST.Shortcut)
                            {
                                MyRadialMenu.RadialMenuItem(sh.Title, (n) => ExecuteCommand(sh.Command));
                            }
                            else if (sh.Type == ST.Menu)
                            {
                                RenderWheel(sh);
                            }
                        }
                        MyRadialMenu.EndRadialMenu();
                    }
                }
                if (!open)
                {
                    Config.IsOpen = false;
                }
            }
        }
        private Shortcut NewShortcut()
        {
            return new()
            {
                Type = (ST)1,
                sublist = new(),
                Title = "",
                Command = ""
            };
        }

        private Shortcut Retree(Shortcut sh, int id_num)
        {
            ImGui.PushID(id_num);
            if (ImGui.TreeNode(sh.UUID.ToString(), sh.Title))
            {
                ImGui.InputText("Title", ref sh.Title, 20);
                int typ = (int)sh.Type;
                ImGui.Combo("Type", ref typ, Enum.GetNames(typeof(ST)), 2);
                sh.Type = (ST)typ;
                if (sh.Type == ST.Menu)
                {
                    for (int i = 0; i < sh.sublist.Count; i++)
                    {
                        ImGui.PushID(i);
                        if (ImGui.Button("X"))
                        {
                            sh.sublist.RemoveAt(i);
                        }
                        else
                        {
                            ImGui.SameLine();
                            sh.sublist[i] = Retree(sh.sublist[i], i);
                        }
                        ImGui.PopID();
                    }
                    if (ImGui.Button("+"))
                    {
                        sh.sublist.Add(NewShortcut());
                    }
                }
                else
                {
                    ImGui.InputText("Command", ref sh.Command, 40);
                }
                ImGui.TreePop();
            }
            ImGui.PopID();
            return sh;
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
                    ImGui.SameLine();
                    if (ImGui.TreeNode(Config.UUID.ToString(), Config.Name))
                    {
                        ImGui.InputText("Name", ref Config.Name, 20);
                        Config.key.Render();
                        if (ImGui.TreeNode(Config.UUID.ToString(), Config.Name))
                        {
                            for (int i = 0; i < Config.RootMenu.Count; i++)
                            {
                                ImGui.PushID(i);
                                if (ImGui.Button("X"))
                                {
                                    Config.RootMenu.RemoveAt(i);
                                }
                                else
                                {
                                    ImGui.SameLine();
                                    Config.RootMenu[i] = Retree(Config.RootMenu[i], i);
                                }
                                ImGui.PopID();
                            }
                            if (ImGui.Button("+"))
                            {
                                Config.RootMenu.Add(NewShortcut());
                            }
                            ImGui.TreePop();
                        }
                        ImGui.TreePop();
                    }
                    ImGui.PopID();
                }
                if (ImGui.Button("+"))
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
            Dalamud.Initialize(dpi);
            Configuration = (Wheels)Dalamud.PluginInterface.GetPluginConfig() ?? new();
            commandManager = new PluginCommandManager<MZRadialMenu>(this);
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
            Dalamud.PluginInterface.SavePluginConfig(Configuration);
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
            Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
            Dalamud.ClientState.Login -= handleLogin;
            commandManager.Dispose();
        }
    }
}