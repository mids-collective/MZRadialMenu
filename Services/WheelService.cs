using ImGuiNET;

namespace MZRadialMenu.Services;

public sealed class WheelService : IDisposable
{
    public static WheelService Instance => Service<WheelService>.Instance;
    public ConfigFile? ActiveConfig;
    private WheelService() { }
    public void Initialize()
    {
        DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
        ActiveConfig = (ConfigFile?)DalamudApi.PluginInterface.GetPluginConfig() ?? new();
    }
    private void Draw()
    {
        if (DalamudApi.ClientState.IsLoggedIn)
        {
            for (int i = 0; i < ActiveConfig!.WheelSet.Count; i++)
            {
                var Config = ActiveConfig.WheelSet[i];
                if (Config.key.key != 0x0)
                {
                    var open = DalamudApi.Keys[Config.key.key];
                    if (open && !ActiveConfig.WheelSet.Any(x => x.IsOpen) && !UIService.Instance.IsGameTextInputActive)
                    {
                        Config.IsOpen = true;
                        ImGui.OpenPopup("##Wheel", ImGuiPopupFlags.NoOpenOverExistingPopup);
                    }
                    Config.Render(open);
                    if (!open)
                    {
                        Config.IsOpen = false;
                    }
                }
            }
        }
    }
    public void Dispose()
    {
        DalamudApi.PluginInterface.UiBuilder.Draw -= Draw;
    }
}