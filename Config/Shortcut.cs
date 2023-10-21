using ImGuiNET;
using ImComponents;

using Plugin.Services;

namespace MZRadialMenu.Config;
public class Shortcut : BaseItem
{
    public override void RenderConfig()
    {
        ImGui.InputText($"Title##{GetID()}", ref Title, 0xF);
        ImGui.InputText($"Command##{GetID()}", ref Command, 0x40);
    }
    public void Execute()
    {
        CmdService.Execute(Command);
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