using ImComponents;
using ImGuiNET;

using Newtonsoft.Json;

using Lumina.Excel.GeneratedSheets;

using Plugin;
using Plugin.Services;
using Dalamud.Utility;

namespace MZRadialMenu.Config;

public class Job : BaseItem
{
    [JsonIgnore]
    public int current_item;
    public override void RenderConfig()
    {
        current_item = cljb.FindIndex(x => x.NameEnglish.ToString() == Title);
        if (current_item == -1)
        {
            current_item = 0;
        }
        ImGui.ListBox("Class / Job", ref current_item, cljb.Select(x => x.NameEnglish.ToString()).ToArray(), cljb.Count);
        Title = cljb[current_item].NameEnglish.ToString();
    }
    public void Execute()
    {
        CmdService.Execute($"/gs change \"{cljb.Where(x => x.NameEnglish.ToString().Equals(Title)).First().NameEnglish}\"");
    }
    public override void Render()
    {
        if (AdvRadialMenu.Instance.RadialMenuItem(Title))
        {
            Execute();
        }
    }
    [JsonIgnore]
    private static List<ClassJob> cljb => DalamudApi.GameData.GetExcelSheet<ClassJob>()!.Where(x => !x.NameEnglish.ToString().Trim().IsNullOrWhitespace()).Where(x => x.Name != "adventurer").OrderBy(x => x.Role).ThenBy(x => x.ClassJobParent.Row).ThenBy(x => x.RowId).ToList();
}