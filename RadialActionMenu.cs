using Dalamud.Plugin;
using MZRadialMenu.Attributes;

using MZRadialMenu.Services;

namespace MZRadialMenu;

public unsafe class MZRadialMenu : IDalamudPlugin
{
    public static MZRadialMenu? Instance;
    private PluginCommandManager<MZRadialMenu> commandManager;
    private List<Action> DisposeActions = new();
    public MZRadialMenu(DalamudPluginInterface dpi)
    {
        DalamudApi.Initialize(dpi);
        Instance = this;
        UIService.Instance.Initialize();
        DisposeActions.Add(UIService.Instance.Dispose);
        ItemService.Instance.Initialize();
        DisposeActions.Add(ItemService.Instance.Dispose);
        MacroService.Instance.Initialize();
        DisposeActions.Add(MacroService.Instance.Dispose);
        CmdService.Instance.Initialize();
        DisposeActions.Add(CmdService.Instance.Dispose);
        WheelService.Instance.Initialize();
        DisposeActions.Add(WheelService.Instance.Dispose);
        ConfigService.Instance.Initialize();
        DisposeActions.Add(ConfigService.Instance.Dispose);
        commandManager = new PluginCommandManager<MZRadialMenu>(this);
        DisposeActions.Add(commandManager.Dispose);
    }

    [Command("/pwheels")]
    [HelpMessage("Show or hide plugin configuation")]
    private void ToggleConfig(string cmd, string args)
    {
        ConfigService.Instance.ToggleConfig();
    }

    public void Dispose()
    {
        foreach(Action itm in DisposeActions) {
            itm.Invoke();
        }
    }
}