using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace MZRadialMenu.Services;

public unsafe sealed class ItemService : IDisposable
{
    public static ItemService Instance => Service<ItemService>.Instance;
    private const int aetherCompassID = 2001886;
    private delegate* unmanaged<nint, uint, uint, uint, short, void> useItem;
    private Dictionary<uint, string> usables = new();
    private nint itemContextMenuAgent;
    private ItemService() { 
        DalamudApi.GameInteropProvider.InitializeFromAttributes(this);
        useItem = (delegate* unmanaged<nint, uint, uint, uint, short, void>)DalamudApi.SigScanner.ScanModule("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 7C 24 38");
        InitUsables();
    }

    public void UseItem(string name)
    {
        if (usables.Count == 0) InitUsables();
        if (string.IsNullOrWhiteSpace(name)) return;

        var newName = name.Replace("\uE03C", ""); // Remove HQ Symbol
        var useHQ = newName != name;
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
    public void Dispose() {
        usables.Clear();
    }
}