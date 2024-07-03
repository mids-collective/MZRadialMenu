using System.Linq;
using ImGuiNET;

namespace Plugin.Services;

public sealed class WheelService : IService<WheelService>
{
    public static WheelService Instance => Service<WheelService>.Instance;
    private ConfigFile _config;
    private WheelService()
    {
        _config = ConfigService.Config();
        DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
    }
    public ConfigFile GetConfig()
    {
        return _config.DeepCopy();
    }
    public void SetConfig(ConfigFile config)
    {
        _config = config.DeepCopy();
    }
    private void Draw()
    {
        if (DalamudApi.ClientState.IsLoggedIn)
        {
            for (int i = 0; i < _config!.WheelSet.Count; i++)
            {
                var Config = _config.WheelSet[i];
                if (Config.key.key != 0x0)
                {
                    var open = DalamudApi.Keys[Config.key.key];
                    if (open && !_config.WheelSet.Any(x => x.IsOpen) && !UIService.Instance.IsGameTextInputActive)
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