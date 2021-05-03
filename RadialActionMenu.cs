using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using Dalamud.Plugin;
using ImComponents;
using ImGuiNET;
using Microsoft.Win32;
using MZRadialMenu.Attributes;

namespace MZRadialMenu
{
    public class MZRadialMenu : IDalamudPlugin
    {
        private Wheel Config;
        public static DalamudPluginInterface interf { get; private set; }
        public static MZRadialMenu Host;
        private PluginCommandManager commandManager;
        public static IntPtr textActiveBoolPtr = IntPtr.Zero;
        public static unsafe bool GameTextInputActive => (textActiveBoolPtr != IntPtr.Zero) && *(bool*)textActiveBoolPtr;
        private bool ConfigOpen = false;
        private void RenderWheel(Shortcut ct)
        {
            if (AdvRadialMenu.BeginRadialMenu(ct.Title))
            {
                foreach (var sh in ct.sublist)
                {
                    if (sh.Type == ST.Shortcut)
                    {
                        var cmd = sh.Command;
                        AdvRadialMenu.RadialMenuItem(sh.Title, (n) => ExecuteCommand(cmd));
                    }
                    else if (sh.Type == ST.Menu)
                    {
                        RenderWheel(sh);
                    }
                }
                AdvRadialMenu.EndRadialMenu();
            }
        }
        private void RenderWheel()
        {
            var open = Keyboard.IsPressed(Keyboard.GetKeyboard()[Config.key]) && !GameTextInputActive;
            if (open)
            {
                ImGui.OpenPopup("##Wheel");
            }
            if (AdvRadialMenu.BeginRadialPopup("##Wheel", open))
            {
                foreach (var sh in Config.RootMenu)
                {
                    if (sh.Type == ST.Shortcut)
                    {
                        var cmd = sh.Command;
                        AdvRadialMenu.RadialMenuItem(sh.Title, (n) => ExecuteCommand(cmd));
                    }
                    else if (sh.Type == ST.Menu)
                    {
                        RenderWheel(sh);
                    }
                }
                AdvRadialMenu.EndRadialMenu();
            }
            AdvRadialMenu.EndRadialPopup();
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
            if (ImGui.TreeNode(""))
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
                            ImGui.Text(sh.sublist[i].Title);
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
                        ImGui.Text(Config.RootMenu[i].Title);
                        ImGui.SameLine();
                        Config.RootMenu[i] = Retree(Config.RootMenu[i], i);
                    }
                    ImGui.PopID();
                }
                if (ImGui.Button("+"))
                {
                    Config.RootMenu.Add(NewShortcut());
                }
                ImGui.SameLine();
                if (ImGui.Button("Save"))
                {
                    interf.SavePluginConfig(Config);
                }
                ImGui.SameLine();
                if (ImGui.Button("Save and Close"))
                {
                    interf.SavePluginConfig(Config);
                    ConfigOpen = false;
                }
                ImGui.End();
            }
        }

        public string Name { get; private set; } = "MZRadialMenu";
        private void Draw()
        {
            ConfigRender();
            RenderWheel();
            //if (interf.IsDebugging && ConfigOpen) AdvRadialMenu.DebugMenu();
        }
        public void Initialize(DalamudPluginInterface dpi)
        {
            interf = dpi;
            Host = this;
            commandManager = new PluginCommandManager();
            Config = (Wheel)interf.GetPluginConfig() ?? new Wheel();
            InitPointers();
            interf.UiBuilder.OnBuildUi += Draw;
            interf.UiBuilder.OnOpenConfigUi += (s,c) => ToggleConfig();
            interf.ClientState.OnLogin += (sender, args) =>
            {
                InitCommands();
                if (uiModule != null && uiModule != IntPtr.Zero)
                    PluginLog.Log("Commands Initialized!");
                else
                    PluginLog.Log("Commands Initialization Failed!");
            };
        }
        private void ToggleConfig() {
            ConfigOpen = !ConfigOpen;
        }
        [Command("/pwheels")]
        [HelpMessage("Show or hide plugin configuation")]
        private void ToggleConfig(string cmd, string args)
        {
            ToggleConfig();
        }
        public void Dispose()
        {
            interf.UiBuilder.OnBuildUi -= Draw;
            commandManager.Dispose();
        }
        private unsafe void InitPointers()
        {
            var dataptr = interf.TargetModuleScanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 48 8B 48 28 80 B9 8E 18 00 00 00");
            if (dataptr != IntPtr.Zero)
                textActiveBoolPtr = *(IntPtr*)(*(IntPtr*)dataptr + 0x28) + 0x188E;
        }
        private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
        private delegate IntPtr GetModuleDelegate(IntPtr basePtr);
        private ProcessChatBoxDelegate ProcessChatBox;
        private IntPtr uiModule = IntPtr.Zero;
        private void InitCommands()
        {
            try
            {
                ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(interf.TargetModuleScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));
                uiModule = interf.Framework.Gui.GetUIModule();
            }
            catch
            {
                PluginLog.Error("Error with loading signatures");
            }
        }

        public void ExecuteCommand(string command)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(command + "\0");
                var memStr = Marshal.AllocHGlobal(0x18 + bytes.Length);

                Marshal.WriteIntPtr(memStr, memStr + 0x18); // String pointer
                Marshal.WriteInt64(memStr + 0x8, bytes.Length); // Byte capacity (unused)
                Marshal.WriteInt64(memStr + 0x10, bytes.Length); // Byte length
                Marshal.Copy(bytes, 0, memStr + 0x18, bytes.Length); // String

                ProcessChatBox(uiModule, memStr, IntPtr.Zero, 0);

                Marshal.FreeHGlobal(memStr);
            }
            catch
            {
                PluginLog.Error("Error with injecting command");
            }
        }
    }
}