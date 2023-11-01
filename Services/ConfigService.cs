using ImGuiNET;

using Dalamud.Game.ClientState.Keys;
using Newtonsoft.Json;
using System.Text;

using MZRadialMenu.Config;
using System.Text.RegularExpressions;

namespace Plugin.Services;

public sealed class ConfigService : IService<ConfigService>
{
    public static ConfigService Instance => Service<ConfigService>.Instance;
    public static ConfigFile Config() => Instance.GetConfig();
    private ConfigFile _config;
    private bool ConfigOpen = false;
    private ConfigService()
    {
        _config = (ConfigFile?)DalamudApi.PluginInterface.GetPluginConfig() ?? new();
        DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        CmdMgrService.Command("/wheels", ToggleConfig, "Toggle Configuration GUI");
    }

    private ConfigFile GetConfig()
    {
        return _config.DeepCopy();
    }
    private void ToggleConfig(string cmd, string args)
    {
        ToggleConfig();
    }
    private void PopupCB(IMenu item)
    {
        int i = _config.WheelSet.FindIndex(x => x.GetID() == item.GetID());
        ImGui.SameLine();
        if (ImGui.Button($"Delete wheel##{item.GetID()}"))
        {
            ImGui.CloseCurrentPopup();
            _config.WheelSet.RemoveAt(i);
        }
    }
    private void DrawMenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Wheels"))
            {
                if (ImGui.MenuItem("New Wheel"))
                {
                    var wheel = new Wheel();
                    wheel.SetTitle("New Wheel");
                    _config.WheelSet.Add(new Wheel());
                }
                if (ImGui.MenuItem("Import Wheel"))
                {
                    var clip = ImGui.GetClipboardText();
                    var regex = new Regex(@"MZRW_\((.*)\)");
                    var matches = regex.Matches(clip);
                    foreach (var match in matches.ToHashSet())
                    {
                        var obj = JsonConvert.DeserializeObject<Wheel>(Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups[1].Captures[0].Value)));
                        if (obj != null)
                        {
                            obj.ResetID();
                            obj.key.key = VirtualKey.NO_KEY;
                            _config.WheelSet.Add(obj);
                        }
                    }
                }
                if (ImGui.MenuItem("Save and Close"))
                {
                    ConfigOpen = false;
                    WheelService.Instance.SetConfig(_config);
                    DalamudApi.PluginInterface.SavePluginConfig(_config);
                }
                if (ImGui.MenuItem("Save"))
                {
                    WheelService.Instance.SetConfig(_config);
                    DalamudApi.PluginInterface.SavePluginConfig(_config);
                }
                if (ImGui.MenuItem("Revert"))
                {
                    _config = WheelService.Instance.GetConfig();
                }
                if (ImGui.MenuItem("Close"))
                {
                    ConfigOpen = false;
                    _config = WheelService.Instance.GetConfig();
                }
                ImGui.EndMenu();
            }
            ImGui.EndMenuBar();
        }
    }
    private void Draw()
    {
        if (DalamudApi.PluginInterface.IsDevMenuOpen)
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.MenuItem("Wheel Menu"))
                {
                    ToggleConfig();
                }
                ImGui.EndMainMenuBar();
            }
        }
        if (ConfigOpen)
        {
            if (ImGui.Begin("MZ Radial Menu Config", ref ConfigOpen, ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();
                for (int c = 0; c < _config!.WheelSet.Count; c++)
                {
                    var Item = _config.WheelSet[c];
                    Item.Config(PopupCB);
                }
                ImGui.End();
            }
        }
    }

    public void ToggleConfig()
    {
        ConfigOpen = !ConfigOpen;
    }

    public void Dispose()
    {
        DalamudApi.PluginInterface.SavePluginConfig(_config);
        DalamudApi.PluginInterface.UiBuilder.Draw -= Draw;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
    }
}