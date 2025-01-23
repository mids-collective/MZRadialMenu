using System;
using System.Text;
using Dalamud.Game.ClientState.Keys;
using ImComponents.Raii;
using ImGuiNET;
using Newtonsoft.Json;
using Plugin;
using Plugin.Attributes;

namespace MZRadialMenu.Config;

[Hidden]
public class Wheel : Menu
{
    public Wheel() : base()
    {
        RegisterCallback(WheelPopup);
    }
    private void WheelPopup(IMenu item)
    {
        key.Render();
        if (ImGui.Button($"Export Wheel##{item.GetID()}"))
        {
            var cpy = (Wheel)item.DeepCopy();
            cpy.ClearID();
            cpy.key.key = VirtualKey.NO_KEY;
            var json = JsonConvert.SerializeObject(cpy);
            var exp = $"MZRW_({Convert.ToBase64String(Encoding.UTF8.GetBytes(json))})";
            ImGui.SetClipboardText(exp);
        }
    }
    public override void Render(ImComponents.Raii.IMenu im)
    {
        foreach (var itm in Sublist)
        {
            itm.Render(im);
        }
    }
    public void Render(bool open)
    {
        if (IsOpen)
        {
            using var rm = new RadialMenu(open);
            if(rm.open)
                Render(rm);
        }
    }
    public HotkeyButton key = new();
    [JsonIgnore]
    public bool IsOpen = false;
}