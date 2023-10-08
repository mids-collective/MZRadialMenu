using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.Game.Command;

namespace MZRadialMenu.Attributes;

#region PluginCommandManager
public class PluginCommandManager<T> : IDisposable where T : IDalamudPlugin
{
    private readonly T _plugin;
    private readonly (string, CommandInfo)[] _pluginCommands;
    public PluginCommandManager(T plugin)
    {
        _plugin = plugin;
        _pluginCommands = _plugin.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
            .SelectMany(GetCommandInfoTuple)
            .ToArray();

        AddCommandHandlers();
    }
    private void AddCommandHandlers()
    {
        foreach (var (command, commandInfo) in _pluginCommands)
            Dalamud.Commands.AddHandler(command, commandInfo);
    }
    private void RemoveCommandHandlers()
    {
        foreach (var (command, _) in _pluginCommands)
            Dalamud.Commands.RemoveHandler(command);
    }
    private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(MethodInfo method)
    {
        var handlerDelegate = (CommandInfo.HandlerDelegate)Delegate.CreateDelegate(typeof(CommandInfo.HandlerDelegate), _plugin, method);

        var command = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>();
        var aliases = handlerDelegate.Method.GetCustomAttribute<AliasesAttribute>();
        var helpMessage = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();
        var doNotShowInHelp = handlerDelegate.Method.GetCustomAttribute<DoNotShowInHelpAttribute>();

        var commandInfo = new CommandInfo(handlerDelegate)
        {
            HelpMessage = helpMessage?.HelpMessage ?? string.Empty,
            ShowInHelp = doNotShowInHelp == null,
        };

        // Create list of tuples that will be filled with one tuple per alias, in addition to the base command tuple.
        var commandInfoTuples = new List<(string, CommandInfo)> { (command!.Command, commandInfo) };
        if (aliases != null)
            commandInfoTuples.AddRange(aliases.Aliases.Select(alias => (alias, commandInfo)));

        return commandInfoTuples;
    }

    public void Dispose()
    {
        RemoveCommandHandlers();
        GC.SuppressFinalize(this);
    }
}
#endregion

#region Attributes
[AttributeUsage(AttributeTargets.Method)]
public class AliasesAttribute : Attribute
{
    public string[] Aliases { get; }

    public AliasesAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public string Command { get; }

    public CommandAttribute(string command)
    {
        Command = command;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DoNotShowInHelpAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class HelpMessageAttribute : Attribute
{
    public string HelpMessage { get; }

    public HelpMessageAttribute(string helpMessage)
    {
        HelpMessage = helpMessage;
    }
}
#endregion
