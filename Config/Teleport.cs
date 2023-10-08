using System.Linq;
using MZRadialMenu.Attributes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using ImComponents;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace MZRadialMenu.Config
{
    [WheelType("Teleport", false)]
    public class Teleport : BaseItem
    {
        public unsafe void Execute()
        {
            if (Dalamud.ClientState.LocalPlayer == null)
            {
                return;
            }
            var status = ActionManager.Instance()->GetActionStatus(ActionType.Action, 5);
            if (status != 0)
            {
                return;
            }
            if (Dalamud.ClientState.LocalPlayer.HomeWorld.Id != Dalamud.ClientState.LocalPlayer.CurrentWorld.Id && this.IsHouse)
            {
                Dalamud.Chat.PrintError($"Cannot teleport to housing while using visiting other worlds!");
            }
            Telepo.Instance()->Teleport(this.TelepoID, this.TelepoSubID);
            return;
        }
        public override void ReTree()
        {
            ImGui.PushID(this.UUID);
            if (ImGui.TreeNode(this.UUID, this.Title))
            {
                ImGui.InputText("Title", ref this.Title, 0xF);
                if (
                    ImGui.BeginCombo(
                        "Teleport", Dalamud.PluginInterface.Sanitizer.Sanitize(Aetherytes.GetRow(this.TelepoID)!.PlaceName.Value?.Name.ToString()!)
                    )
                )
                {
                    foreach (var itm in Dalamud.AetheryteList
                        .OrderBy(x => Aetherytes.GetRow(x.AetheryteData.GameData!.RowId)!.Territory.Value!.PlaceNameRegion.Row)
                        .ThenBy(x => Aetherytes.GetRow(x.AetheryteData.GameData!.RowId)!.PlaceName.Row)
                    )
                    {
                        if (
                            ImGui.Selectable(
                                Dalamud.PluginInterface.Sanitizer.Sanitize(Aetherytes.GetRow(itm.AetheryteData.GameData!.RowId)!.PlaceName.Value?.Name.ToString()!)
                            )
                        )
                        {
                            this.TelepoID = itm.AetheryteId;
                            this.TelepoSubID = itm.SubIndex;
                            this.IsHouse = itm.IsSharedHouse || itm.IsAppartment || itm.Ward != 0 || itm.Plot != 0;
                        }
                        if (itm.AetheryteId == this.TelepoID && itm.SubIndex == this.TelepoSubID)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            ImGui.PopID();
        }
        public override void Render(AdvRadialMenu radialMenu)
        {
            if (radialMenu.RadialMenuItem(this.Title))
            {
                this.Execute();
            }
        }
        public bool IsHouse = false;
        public uint TelepoID = 0;
        public byte TelepoSubID = 0;
        [JsonIgnore]
        private static Lumina.Excel.ExcelSheet<Aetheryte> Aetherytes = Dalamud.GameData.GetExcelSheet<Aetheryte>(Dalamud.ClientState.ClientLanguage)!;
    }
}