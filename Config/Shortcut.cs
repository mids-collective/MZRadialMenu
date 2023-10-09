using MZRadialMenu.Attributes;
using ImGuiNET;
using ImComponents;

namespace MZRadialMenu.Config;

[WheelType("Shortcut", false)]
public class Shortcut : BaseItem
{
    public override bool RenderConfig()
    {
        ImGui.PushID(this.UUID);
        if (ImGui.TreeNode(this.UUID, this.Title))
        {
            ImGui.InputText("Title", ref this.Title, 0xF);
            ImGui.InputText("Command", ref Command, 0x40);
            ImGui.TreePop();
        }
        ImGui.PopID();
        return true;
    }
    public void Execute()
    {
        MZRadialMenu.Instance!.ExecuteCommand(this.Command);
    }
    public override void Render(AdvRadialMenu radialMenu)
    {
        if (radialMenu.RadialMenuItem(this.Title))
        {
            this.Execute();
        }
    }
    public string Command = string.Empty;
}