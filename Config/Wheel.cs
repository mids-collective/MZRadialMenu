using MZRadialMenu.Attributes;
using ImComponents;
using ImGuiNET;
using Newtonsoft.Json;

namespace MZRadialMenu.Config;

[WheelType("Wheel", true)]
public class Wheel : Menu
{
    public override void Render(AdvRadialMenu radialMenu)
    {
        foreach (var itm in Sublist)
        {
            itm.Render(radialMenu);
        }
    }
    public void Render(AdvRadialMenu radialMenu, bool open)
    {
        if (IsOpen)
        {
            if (radialMenu.BeginRadialPopup("##Wheel", open))
            {
                Render(radialMenu);
                radialMenu.EndRadialMenu();
            }
        }
    }
    public override bool RenderConfig()
    {
        bool show_buttons = true;
        ImGui.PushID(UUID);
        if (ImGui.TreeNode(UUID, Title))
        {
            show_buttons = false;
            key.Render();
            show_buttons &= base.RawRender();
            ImGui.TreePop();
        }
        ImGui.PopID();
        return show_buttons;
    }
    public HotkeyButton key = new();
    [JsonIgnore]
    public bool IsOpen = false;
}