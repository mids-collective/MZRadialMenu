using System.Collections.Generic;
using Dalamud.Game.Command;

namespace Plugin.Services;
public sealed class CmdMgrService : IService<CmdMgrService>
{
    public static CmdMgrService Instance => Service<CmdMgrService>.Instance;
    public static void Command(string cmd, IReadOnlyCommandInfo.HandlerDelegate handle, string helpMessage = "Message to show in help", bool showInHelp = true, string[]? aliases = null) => Instance.AddCommand(cmd, handle, helpMessage, showInHelp, aliases);
    private HashSet<string> pluginCommands;
    private CmdMgrService()
    {
        pluginCommands = new();
    }
    public void AddCommand(string cmd, IReadOnlyCommandInfo.HandlerDelegate handle, string helpMessage = "Message to show in help", bool showInHelp = true, string[]? aliases = null)
    {
        var cmdInfo = new CommandInfo(handle)
        {
            HelpMessage = helpMessage,
            ShowInHelp = showInHelp,
        };
        if (DalamudApi.Commands.AddHandler(cmd, cmdInfo))
        {
            pluginCommands.Add(cmd);
        }
        if (aliases != null)
        {
            foreach (var itm in aliases)
            {
                if (DalamudApi.Commands.AddHandler(itm, cmdInfo))
                {
                    pluginCommands.Add(itm);
                }
            }
        }
    }
    public void Dispose()
    {
        foreach (var cmd in pluginCommands)
        {
            DalamudApi.Commands.RemoveHandler(cmd);
        }
    }
}