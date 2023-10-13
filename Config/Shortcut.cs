using ImGuiNET;
using ImComponents;

using Plugin.Attributes;
using Plugin.Services;

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
        CmdService.Instance.ExecuteCommand(Command);
    }
    public override void Render()
    {
        if (AdvRadialMenu.Instance.RadialMenuItem(Title))
        {
            Execute();
        }
    }
    public string Command = string.Empty;
}