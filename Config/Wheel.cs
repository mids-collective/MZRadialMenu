using ImComponents;
using ImGuiNET;

using Newtonsoft.Json;

using MZRadialMenu.Attributes;

namespace MZRadialMenu.Config;

[WheelType("Wheel", true)]
public class Wheel : Menu
{
    public override void Render()
    {
        foreach (var itm in Sublist)
        {
            itm.Render();
        }
    }
    public void Render(bool open)
    {
        if (IsOpen)
        {
            if (AdvRadialMenu.Instance.BeginRadialPopup("##Wheel", open))
            {
                Render();
                AdvRadialMenu.Instance.EndRadialMenu();
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