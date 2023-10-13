using ImGuiNET;

using Dalamud.Game.ClientState.Keys;
using Newtonsoft.Json;
using System.Text;

using MZRadialMenu.Config;

namespace Plugin.Services;

public sealed class ConfigService : IService<ConfigService>
{
    public static ConfigService Instance => Service<ConfigService>.Instance;
    private ConfigFile Config;
    private bool ConfigOpen = false;
    private ConfigService()
    {
        Config = (ConfigFile?)DalamudApi.PluginInterface.GetPluginConfig() ?? new();
        DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        CmdMgrService.Instance.AddCommand("/wheels", ToggleConfig, "Toggle Configuration GUI");
    }

    public ConfigFile GetConfig()
    {
        return Config;
    }
    private void ToggleConfig(string cmd, string args)
    {
        ToggleConfig();
    }

    private void Draw()
    {
        if (ConfigOpen)
        {
            ImGui.Begin("MZ Radial Menu Config", ref ConfigOpen);
            var size = ImGui.GetContentRegionAvail();
            size.Y -= 30;
            ImGui.BeginChild("Configuration", size);
            for (int c = 0; c < Config!.WheelSet.Count; c++)
            {
                var Item = Config.WheelSet[c];
                ImGui.PushID(Item.UUID.ToString());
                if (ImGui.Button("X"))
                {
                    Config.WheelSet.RemoveAt(c);
                }
                ImGui.SameLine();
                if (ImGui.Button("Export Wheel"))
                {
                    var cpy = Item.DeepCopy();
                    cpy.UUID = string.Empty;
                    cpy.key.key = VirtualKey.NO_KEY;
                    var json = JsonConvert.SerializeObject(cpy);
                    var exp = $"MZRW_({Convert.ToBase64String(Encoding.UTF8.GetBytes(json))})";
                    ImGui.SetClipboardText(exp);
                }
                ImGui.SameLine();
                if (ImGui.TreeNode(Item.UUID.ToString(), Item.Title))
                {
                    Item.key.Render();
                    Item.RawRender();
                    ImGui.TreePop();
                }
                ImGui.PopID();
            }
            ImGui.EndChild();
            ImGui.Separator();
            if (ImGui.Button("New Wheel"))
            {
                Config.WheelSet.Add(new Wheel());
            }
            ImGui.SameLine();
            if (ImGui.Button("Import Wheel"))
            {
                var clip = ImGui.GetClipboardText();
                if (clip.StartsWith("MZRW_("))
                {
                    clip = clip[6..^1];
                    var obj = JsonConvert.DeserializeObject<Wheel>(Encoding.UTF8.GetString(Convert.FromBase64String(clip)))!;
                    obj.UUID = Guid.NewGuid().ToString();
                    obj.key.key = VirtualKey.NO_KEY;
                    Config.WheelSet.Add(obj);
                }
            }
            ImGui.SameLine();
            var pos = ImGui.GetCursorPos();
            pos.X = size.X - 220;
            ImGui.SetCursorPos(pos);
            if (ImGui.Button("Save and Close"))
            {
                ConfigOpen = false;
                WheelService.Instance.SetConfig(Config);
            }
            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                WheelService.Instance.SetConfig(Config);
                DalamudApi.PluginInterface.SavePluginConfig(Config);
            }
            ImGui.SameLine();
            if (ImGui.Button("Revert"))
            {
                Config = WheelService.Instance.GetConfig().DeepCopy();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ConfigOpen = false;
                Config = WheelService.Instance.GetConfig().DeepCopy();
            }
            ImGui.End();
        }
    }

    public void ToggleConfig()
    {
        ConfigOpen = !ConfigOpen;
    }

    public void Dispose()
    {
        DalamudApi.PluginInterface.UiBuilder.Draw -= Draw;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
    }
}