using ImComponents;
using ImGuiNET;

using Newtonsoft.Json;

using Plugin;
using Plugin.Services;
using Dalamud.Utility;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace MZRadialMenu.Config;

public class Job : BaseItem
{
    [JsonIgnore]
    public int current_item;
    public Job() {
        Title = cljb.First().NameEnglish.ToString();
    }
    public override void RenderConfig()
    {
        current_item = cljb.FindIndex(x => x.NameEnglish.ToString() == Title);
        ImGui.ListBox("Class / Job", ref current_item, cljb.Select(x => x.NameEnglish.ToString()).ToArray(), cljb.Count);
        Title = cljb[current_item].NameEnglish.ToString();
    }
    public void Execute()
    {
        CmdService.Execute($"/gs change \"{cljb.Where(x => x.NameEnglish.ToString().Equals(Title)).First().NameEnglish}\"");
    }
    public override void Render()
    {
        if (RadialMenu.Instance.RadialMenuItem(Title))
        {
            Execute();
        }
    }
    [JsonIgnore]
    private static List<ClassJob> cljb => DalamudApi.GameData.GetExcelSheet<ClassJob>()!.Where(x => !x.NameEnglish.ToString().Trim().IsNullOrWhitespace()).Where(x => x.Name != "adventurer").OrderBy(x => x.Role).ThenBy(x => x.ClassJobParent.RowId).ThenBy(x => x.RowId).ToList();
}