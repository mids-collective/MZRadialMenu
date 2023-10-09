using MZRadialMenu.Attributes;
using ImGuiNET;
using ImComponents;

namespace MZRadialMenu.Config;

[WheelType("Shortcut", false)]
public class Shortcut : BaseItem
{
    public override bool RenderConfig()
    {
        ImGui.PushID(UUID);
        if (ImGui.TreeNode(UUID, Title))
        {
            ImGui.InputText("Title", ref Title, 0xF);
            ImGui.InputText("Command", ref Command, 0x40);
            ImGui.TreePop();
        }
        ImGui.PopID();
        return true;
    }
    public void Execute()
    {
        MZRadialMenu.Instance!.ExecuteCommand(Command);
    }
    public override void Render(AdvRadialMenu radialMenu)
    {
        if (radialMenu.RadialMenuItem(Title))
        {
            Execute();
        }
    }
    public string Command = string.Empty;
}