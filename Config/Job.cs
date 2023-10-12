using ImComponents;
using ImGuiNET;
using MZRadialMenu.Attributes;
using Newtonsoft.Json;
using Lumina.Excel.GeneratedSheets;
using MZRadialMenu.Services;

namespace MZRadialMenu.Config;

[WheelType("Job", false)]
public class Job : BaseItem
{
    public override bool RenderConfig()
    {
        ImGui.PushID(UUID);
        if (ImGui.BeginCombo("Job / Class", Title))
        {
            foreach (var cjb in cljb.Where(x => x.Name != "adventurer").OrderBy(x => x.Role).ThenBy(x => x.ClassJobParent.Row).ThenBy(x => x.RowId))
            {
                if (ImGui.Selectable(cjb.NameEnglish.ToString()))
                {
                    Title = cjb.NameEnglish.ToString();
                }
                if (Title.Equals(cjb.NameEnglish.ToString()))
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
        ImGui.PopID();
        return true;
    }
    public override void Render()
    {
        if (AdvRadialMenu.Instance.RadialMenuItem(Title))
        {
            DalamudApi.PluginLog.Debug($"{cljb.Where(x => x.NameEnglish.ToString().Equals(Title)).First().NameEnglish.ToString()}");
            CmdService.Instance.ExecuteCommand($"/gs change \"{cljb.Where(x => x.NameEnglish.ToString().Equals(Title)).First().NameEnglish.ToString()}\"");
        }
    }
    [JsonIgnore]
    private static Lumina.Excel.ExcelSheet<ClassJob> cljb = DalamudApi.GameData.GetExcelSheet<ClassJob>()!;
}