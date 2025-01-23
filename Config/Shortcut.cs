using ImGuiNET;
using ImComponents;

using Plugin.Services;

namespace MZRadialMenu.Config;
public class Shortcut : BaseItem, ITemplatable
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
    public override void Render(ImComponents.Raii.IMenu im)
    {
        if (im.RadialMenuItem(Title))
        {
            Execute();
        }
    }

    public void RenderTemplate(TemplateObject reps, ImComponents.Raii.IMenu im)
    {
        if(im.RadialMenuItem(Title)) {
            ExecuteTemplate(reps.repl);
        }
    }

    public void ExecuteTemplate(string reps)
    {
        var cmd = Command.Clone().ToString()!;
        cmd = cmd.Replace($"<tmpl>", $"{reps}");
        CmdService.Execute(cmd);
    }

    public string Command = string.Empty;
}