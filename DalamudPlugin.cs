using System;
using Dalamud.Plugin;
using Plugin;

namespace MZRadialMenu;

public class MZRadialMenu : IDalamudPlugin
{
    private ServiceInitializer initializer;
    public MZRadialMenu(IDalamudPluginInterface dpi)
    {
        DalamudApi.Initialize(dpi);
        initializer = new ServiceInitializer();
    }

    public void Dispose()
    {
        initializer.Dispose();
    }
}