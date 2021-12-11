using System.Linq;
using MZRadialMenu.Attributes;
using ImGuiNET;
using Newtonsoft.Json;
using Dalamud.Logging;
using ImComponents;
using Lumina.Excel.GeneratedSheets;
namespace MZRadialMenu.Config {
    [WheelType(typeof(Job))]
    public class Job : BaseItem
    {
        public override void ReTree()
        {
            ImGui.PushID(this.UUID);
            if (ImGui.BeginCombo("Job / Class", this.Title))
            {
                foreach (var cjb in cljb)
                {
                    if (ImGui.Selectable(cjb.Name.ToString()))
                    {
                        Title = cjb.Name.ToString();
                    }
                    if(Title.Equals(cjb.Name.ToString())) {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.PopID();
        }
        public override void Render(AdvRadialMenu radialMenu)
        {
            if (radialMenu.RadialMenuItem(this.Title))
            {
                PluginLog.Information(this.Title);
                MZRadialMenu.Instance.ExecuteCommand($"/gearset change {cljb.Where(x => x.Name.ToString().Equals(this.Title)).First().Abbreviation.ToString().ToUpper()}");
            }
        }
        [JsonIgnore]
        private static Lumina.Excel.ExcelSheet<ClassJob> cljb = Dalamud.GameData.GetExcelSheet<ClassJob>()!;
    }
}