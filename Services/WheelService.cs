using ImGuiNET;

namespace Plugin.Services;

public sealed class WheelService : IService<WheelService>
{
    public static WheelService Instance => Service<WheelService>.Instance;
    private ConfigFile Config;
    private WheelService()
    {
        DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
        Config = ConfigService.Instance.GetConfig().DeepCopy();
    }
    public ConfigFile GetConfig()
    {
        return Config;
    }
    public void SetConfig(ConfigFile config)
    {
        Config = config.DeepCopy();
    }
    private void Draw()
    {
        if (DalamudApi.ClientState.IsLoggedIn)
        {
            for (int i = 0; i < Config!.WheelSet.Count; i++)
            {
                var Config = this.Config.WheelSet[i];
                if (Config.key.key != 0x0)
                {
                    var open = DalamudApi.Keys[Config.key.key];
                    if (open && !this.Config.WheelSet.Any(x => x.IsOpen) && !UIService.Instance.IsGameTextInputActive)
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