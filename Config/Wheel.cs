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
        foreach (var itm in this.Sublist)
        {
            itm.Render(radialMenu);
        }
    }
    public void Render(AdvRadialMenu radialMenu, bool open)
    {
        if (this.IsOpen)
        {
            if (radialMenu.BeginRadialPopup("##Wheel", open))
            {
                this.Render(radialMenu);
                radialMenu.EndRadialMenu();
            }
        }
    }
    public override void ReTree()
    {
        ImGui.PushID(this.UUID);
        if (ImGui.TreeNode(this.UUID, this.Title))
        {
            this.key.Render();
            base.RawRender();
            ImGui.TreePop();
        }
        ImGui.PopID();
    }
    public HotkeyButton key = new();
    [JsonIgnore]
    public bool IsOpen = false;
}