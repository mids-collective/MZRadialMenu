using System.Linq;
using MZRadialMenu.Attributes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using ImComponents;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace MZRadialMenu.Config;

[WheelType("Teleport", false)]
public class Teleport : BaseItem
{
    public unsafe void Execute()
    {
        if (DalamudApi.ClientState.LocalPlayer == null)
        {
            return;
        }
        var status = ActionManager.Instance()->GetActionStatus(ActionType.Action, 5);
        if (status != 0)
        {
            return;
        }
        if (DalamudApi.ClientState.LocalPlayer.HomeWorld.Id != DalamudApi.ClientState.LocalPlayer.CurrentWorld.Id && IsHouse)
        {
            DalamudApi.Chat.PrintError($"Cannot teleport to housing while using visiting other worlds!");
        }
        Telepo.Instance()->Teleport(TelepoID, TelepoSubID);
        return;
    }
    public override bool RenderConfig()
    {
        ImGui.PushID(UUID);
        if (ImGui.TreeNode(UUID, Title))
        {
            ImGui.InputText("Title", ref Title, 0xF);
            if (
                ImGui.BeginCombo(
                    "Teleport", DalamudApi.PluginInterface.Sanitizer.Sanitize(Aetherytes.GetRow(TelepoID)!.PlaceName.Value?.Name.ToString()!)
                )
            )
            {
                foreach (var itm in DalamudApi.AetheryteList
                    .OrderBy(x => Aetherytes.GetRow(x.AetheryteData.GameData!.RowId)!.Territory.Value!.PlaceNameRegion.Row)
                    .ThenBy(x => Aetherytes.GetRow(x.AetheryteData.GameData!.RowId)!.PlaceName.Row)
                )
                {
                    if (
                        ImGui.Selectable(
                            DalamudApi.PluginInterface.Sanitizer.Sanitize(Aetherytes.GetRow(itm.AetheryteData.GameData!.RowId)!.PlaceName.Value?.Name.ToString()!)
                        )
                    )
                    {
                        TelepoID = itm.AetheryteId;
                        TelepoSubID = itm.SubIndex;
                        IsHouse = itm.IsSharedHouse || itm.IsAppartment || itm.Ward != 0 || itm.Plot != 0;
                    }
                    if (itm.AetheryteId == TelepoID && itm.SubIndex == TelepoSubID)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
        }
        ImGui.PopID();
        return true;
    }
    public override void Render(AdvRadialMenu radialMenu)
    {
        if (radialMenu.RadialMenuItem(Title))
        {
            Execute();
        }
    }
    public bool IsHouse = false;
    public uint TelepoID = 0;
    public byte TelepoSubID = 0;
    [JsonIgnore]
    private static Lumina.Excel.ExcelSheet<Aetheryte> Aetherytes = DalamudApi.GameData.GetExcelSheet<Aetheryte>(DalamudApi.ClientState.ClientLanguage)!;
}