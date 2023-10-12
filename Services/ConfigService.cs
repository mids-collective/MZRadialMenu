using ImGuiNET;
using MZRadialMenu.Extensions;

using Dalamud.Game.ClientState.Keys;
using Newtonsoft.Json;
using System.Text;

using MZRadialMenu.Config;

namespace MZRadialMenu.Services;

public sealed class ConfigService : IDisposable
{
    public static ConfigService Instance => Service<ConfigService>.Instance;
    private ConfigFile? ConfigWindow;
    private bool ConfigOpen = false;
    private ConfigService() { 
        ConfigWindow = WheelService.Instance.ActiveConfig.DeepCopy();
        DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
    }
    private void Draw()
    {
        if (ConfigOpen)
        {
            ImGui.Begin("MZ Radial Menu Config", ref ConfigOpen);
            var size = ImGui.GetContentRegionAvail();
            size.Y -= 30;
            ImGui.BeginChild("Configuration", size);
            for (int c = 0; c < ConfigWindow!.WheelSet.Count; c++)
            {
                var Item = ConfigWindow.WheelSet[c];
                ImGui.PushID(Item.UUID.ToString());
                if (ImGui.Button("X"))
                {
                    ConfigWindow.WheelSet.RemoveAt(c);
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
                    obj.UUID = Guid.NewGuid().ToString();
                    obj.key.key = VirtualKey.NO_KEY;
                    ConfigWindow.WheelSet.Add(obj);
                }
            }
            ImGui.SameLine();
            var pos = ImGui.GetCursorPos();
            pos.X = size.X - 220;
            ImGui.SetCursorPos(pos);
            if (ImGui.Button("Save and Close"))
            {
                ConfigOpen = false;
                WheelService.Instance.ActiveConfig = ConfigWindow.DeepCopy();
            }
            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                WheelService.Instance.ActiveConfig = ConfigWindow.DeepCopy();
                DalamudApi.PluginInterface.SavePluginConfig(WheelService.Instance.ActiveConfig);
            }
            ImGui.SameLine();
            if (ImGui.Button("Revert"))
            {
                ConfigWindow = WheelService.Instance.ActiveConfig.DeepCopy();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ConfigOpen = false;
                ConfigWindow = WheelService.Instance.ActiveConfig.DeepCopy();
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