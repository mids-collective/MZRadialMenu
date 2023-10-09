using System.Linq;
using ImComponents;
using ImGuiNET;
using MZRadialMenu.Attributes;
using Newtonsoft.Json;
using Lumina.Excel.GeneratedSheets;

namespace MZRadialMenu.Config;

[WheelType("Job", false)]
public class Job : BaseItem
{
    public override bool RenderConfig()
    {
        ImGui.PushID(this.UUID);
        if (ImGui.BeginCombo("Job / Class", this.Title))
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
    public override void Render(AdvRadialMenu radialMenu)
    {
        if (radialMenu.RadialMenuItem(this.Title))
        {
            DalamudApi.PluginLog.Debug($"{cljb.Where(x => x.NameEnglish.ToString().Equals(this.Title)).First().NameEnglish.ToString()}");
            MZRadialMenu.Instance!.ExecuteCommand($"/gs change \"{cljb.Where(x => x.NameEnglish.ToString().Equals(this.Title)).First().NameEnglish.ToString()}\"");
        }
    }
    [JsonIgnore]
    private static Lumina.Excel.ExcelSheet<ClassJob> cljb = DalamudApi.GameData.GetExcelSheet<ClassJob>()!;
}