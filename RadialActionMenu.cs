using Dalamud.Plugin;
using MZRadialMenu.Attributes;

using MZRadialMenu.Services;

namespace MZRadialMenu;

public unsafe class MZRadialMenu : IDalamudPlugin
{
    public static MZRadialMenu? Instance;
    private PluginCommandManager commandManager;
    private List<IDisposable> ServiceList = new();
    public MZRadialMenu(DalamudPluginInterface dpi)
    {
        DalamudApi.Initialize(dpi);
        Instance = this;
        ServiceList.Add(UIService.Instance);
        ServiceList.Add(ItemService.Instance);
        ServiceList.Add(MacroService.Instance);
        ServiceList.Add(CmdService.Instance);
        ServiceList.Add(WheelService.Instance);
        ServiceList.Add(ConfigService.Instance);
        commandManager = new PluginCommandManager(this);
        ServiceList.Add(commandManager);
    }

    [Command("/pwheels")]
    [HelpMessage("Show or hide plugin configuation")]
    private void ToggleConfig(string cmd, string args)
    {
        ConfigService.Instance.ToggleConfig();
    }

    public void Dispose()
    {
        foreach(var service in ServiceList) {
            service.Dispose();
        }
    }
}