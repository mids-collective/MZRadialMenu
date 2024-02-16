using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Plugin.Services;

public unsafe sealed class ItemService : IService<ItemService>
{
    public static ItemService Instance => Service<ItemService>.Instance;
    private const int aetherCompassID = 2001886;
    private static delegate* unmanaged<nint, uint, uint, uint, short, void> useItem;
    private Dictionary<uint, string> usables = new();
    private nint itemContextMenuAgent;
    private ItemService()
    {
        useItem = (delegate* unmanaged<nint, uint, uint, uint, short, void>)DalamudApi.SigScanner.ScanText(SigService.GetSig("UseItem"));
        usables.Clear();
        InitUsables();
    }

    public void Use(string name)
    {
        if (usables.Count == 0) InitUsables();
        if (string.IsNullOrWhiteSpace(name)) return;

        var newName = name.Replace("\uE03C", ""); // Remove HQ Symbol
        var useHQ = !newName.Equals(name);
        newName = newName.ToLower().Trim(' ');

        UseItem(usables.First(x => x.Value == newName).Key + (uint)(useHQ ? 1_000_000 : 0));
    }

    private void UseItem(uint id)
    {
        if (id == 0) return;
        // Aether Compass
        if (id == aetherCompassID)
        {
            ActionManager.Instance()->UseAction(ActionType.Action, 26988);
        }
        else if (id == usables.First(x => x.Value == "wondrous tails").Key)
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
        usables[aetherCompassID] = "aether compass";
        itemContextMenuAgent = UIService.Instance.GetAgentByInternalID(AgentId.InventoryContext);
    }
    public void Dispose()
    {
        usables.Clear();
    }
}