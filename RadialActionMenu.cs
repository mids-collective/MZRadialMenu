using System;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Plugin;
using ImComponents;
using ImGuiNET;
using MZRadialMenu.Attributes;

namespace MZRadialMenu
{
    public class MZRadialMenu : IDalamudPlugin
    {
        private Wheel Config;
        private DalamudPluginInterface interf;
        private PluginCommandManager commandManager;
#if DEBUG
        private bool ConfigOpen = true;
#else
        private bool ConfigOpen = false;
#endif
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
            var open = Keyboard.IsPressed(Keyboard.GetKeyboard()[Config.key]);
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
            commandManager = new PluginCommandManager(dpi, this);
            Config = (Wheel)interf.GetPluginConfig() ?? new Wheel();
            interf.UiBuilder.OnBuildUi += Draw;
            interf.ClientState.OnLogin += (sender, args) =>
            {
                InitCommands();
                if (uiModulePtr != null && uiModulePtr != IntPtr.Zero)
                    PluginLog.Log("Commands Initialized!");
                else
                    PluginLog.Log("Commands Initialization Failed!");
            };
        }
        [Command("/pwheels")]
        [HelpMessage("Show or hide plugin configuation")]
        private void ToggleConfig(string cmd, string args)
        {
            ConfigOpen = !ConfigOpen;
        }
        public void Dispose()
        {
            interf.UiBuilder.OnBuildUi -= Draw;
            commandManager.Dispose();
        }
        private delegate IntPtr GetUIModuleDelegate(IntPtr basePtr);
        private delegate void EasierProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
        private GetUIModuleDelegate GetUIModule;
        private EasierProcessChatBoxDelegate _EasierProcessChatBox;
        public IntPtr uiModulePtr;
        private void InitCommands()
        {
            try
            {
                var getUIModulePtr = interf.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 48 83 7F ?? 00 48 8B F0");
                var easierProcessChatBoxPtr = interf.TargetModuleScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
                uiModulePtr = interf.TargetModuleScanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 48 8D 54 24 ?? 48 83 C1 10 E8 ?? ?? ?? ??");

                GetUIModule = Marshal.GetDelegateForFunctionPointer<GetUIModuleDelegate>(getUIModulePtr);
                _EasierProcessChatBox = Marshal.GetDelegateForFunctionPointer<EasierProcessChatBoxDelegate>(easierProcessChatBoxPtr);
            }
            catch
            {
                PluginLog.Error("Error with loading signatures");
            }
        }

        public void ExecuteCommand(string cmd)
        {
            try
            {
                if (uiModulePtr == null || uiModulePtr == IntPtr.Zero)
                    InitCommands();

                var uiModule = GetUIModule(Marshal.ReadIntPtr(uiModulePtr));

                if (uiModule == IntPtr.Zero)
                {
                    throw new ApplicationException("uiModule was null");
                }

                var command = cmd;

                var bytes = Encoding.UTF8.GetBytes(command);

                var mem1 = Marshal.AllocHGlobal(400);
                var mem2 = Marshal.AllocHGlobal(bytes.Length + 30);

                Marshal.Copy(bytes, 0, mem2, bytes.Length);
                Marshal.WriteByte(mem2 + bytes.Length, 0);
                Marshal.WriteInt64(mem1, mem2.ToInt64());
                Marshal.WriteInt64(mem1 + 8, 64);
                Marshal.WriteInt64(mem1 + 8 + 8, bytes.Length + 1);
                Marshal.WriteInt64(mem1 + 8 + 8 + 8, 0);

                _EasierProcessChatBox(uiModule, mem1, IntPtr.Zero, 0);

                Marshal.FreeHGlobal(mem1);
                Marshal.FreeHGlobal(mem2);
            }
            catch
            {
                PluginLog.Error("Error with injecting command");
            }
        }
    }
}