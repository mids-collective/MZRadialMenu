using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Plugin.Services;

public unsafe sealed class ItemService : IService<ItemService>
{
    public static ItemService Instance => Service<ItemService>.Instance;
    private const int aetherCompassID = 2001886;
    private delegate void UseItemDelegate(nint agent, uint cnt, uint itm, uint unk1, short unk2);
    private UseItemDelegate useItem;
    private delegate uint GetActionID(uint a, uint b);
    private GetActionID getActionID;
    private Dictionary<uint, string> usables = new();
    private nint itemContextMenuAgent;
    private ItemService()
    {
        useItem = Marshal.GetDelegateForFunctionPointer<UseItemDelegate>(DalamudApi.SigScanner.ScanText(SigService.GetSig("UseItem")));
        getActionID = Marshal.GetDelegateForFunctionPointer<GetActionID>(DalamudApi.SigScanner.ScanText(SigService.GetSig("GetActionID")));
        InitUsables();
    }

    public void Use(string name)
    {
        if (usables.Count == 0) InitUsables();
        if (string.IsNullOrWhiteSpace(name)) return;

        var newName = name.Replace("\uE03C", ""); // Remove HQ Symbol
        var useHQ = !newName.Equals(name);
        newName = newName.ToLower().Trim(' ');

        UseItem(usables.First(i => i.Value == newName).Key + (uint)(useHQ ? 1_000_000 : 0));
    }

    private void UseItem(uint id)
    {
        if (id == 0 || !usables.ContainsKey(id is >= 1_000_000 and < 2_000_000 ? id - 1_000_000 : id)) return;
        // Aether Compass
        if (id == aetherCompassID)
        {
            ActionManager.Instance()->UseAction(ActionType.Action, 26988);
        }
        else if (usables[id] == "wondrous tails")
        {
            ActionManager.Instance()->UseAction(ActionType.KeyItem, id);
        }
        else
        {
            id = getActionID(2, id);
            useItem(itemContextMenuAgent, id, 9999, 0, 0);
        }
    }

    private void InitUsables()
    {
        usables = DalamudApi.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.Where(i => i.ItemAction.Row > 0).ToDictionary(i => i.RowId, i => i.Name.ToString().ToLower())
            .Concat(DalamudApi.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.EventItem>()!.Where(i => i.Action.Row > 0).ToDictionary(i => i.RowId, i => i.Name.ToString().ToLower()))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        usables[aetherCompassID] = DalamudApi.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.EventItem>()!.GetRow(aetherCompassID)?.Name.ToString().ToLower()!;
        itemContextMenuAgent = UIService.Instance.GetAgentByInternalID(AgentId.InventoryContext);
    }
    public void Dispose()
    {
        usables.Clear();
    }
}