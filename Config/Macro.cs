using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using ImComponents.Raii;
using ImGuiNET;
using Plugin.Attributes;
using Plugin.Services;
using Plugin.Structures;

namespace MZRadialMenu.Config;

//Virtual macros?
//Need to limit text output macros
[Rename("MacroShortcut")]
public class Macro : BaseItem, ITemplatable
{
    public override void RenderConfig()
    {
        ImGui.InputText($"Title##{GetID()}", ref Title, 0xF);
        ImGui.Text($"Macro Commands");
        var combinds = string.Join('\n', Commands);
        ImGui.InputTextMultiline($"Commands##{GetID()}", ref combinds, 0x41 * 30, new Vector2(ImGui.CalcItemWidth(), 200));
        Commands = combinds.Split('\n').Where(x => !string.IsNullOrEmpty(x)).Take(30).ToList();
    }
    public unsafe void Execute()
    {
        var macroPtr = Marshal.AllocHGlobal(ExtendedMacro.size);
        using var ExMacro = new ExtendedMacro(macroPtr, string.Empty, Commands);
        Marshal.StructureToPtr(ExMacro, macroPtr, false);
        var commandCount = (byte)Math.Max(MacroStruct.numLines, Commands.Count());
        MacroService.Instance.NumCopiedMacroLines = commandCount;
        MacroService.Instance.NumExecutedMacroLines = commandCount;
        MacroService.Instance.ExecuteMacroHook!.Original(UIService.Instance.raptureShellModule, macroPtr);
        MacroService.Instance.NumCopiedMacroLines = MacroStruct.numLines;
        Marshal.FreeHGlobal(macroPtr);
    }
    public override void Render(IMenu im)
    {
        if (im.RadialMenuItem(Title))
        {
            Execute();
        }
    }

    public void RenderTemplate(TemplateObject reps, IMenu im)
    {
        if (im.RadialMenuItem(Title))
        {
            ExecuteTemplate(reps.repl);
        }
    }

    public unsafe void ExecuteTemplate(string reps)
    {
        var lst = Commands.ToArray().ToList();
        lst.ForEach(x =>  x = x.Replace($"<tmpl>", reps));
        var macroPtr = Marshal.AllocHGlobal(ExtendedMacro.size);
        using var ExMacro = new ExtendedMacro(macroPtr, string.Empty, lst);
        Marshal.StructureToPtr(ExMacro, macroPtr, false);
        var commandCount = (byte)Math.Max(MacroStruct.numLines, lst.Count());
        MacroService.Instance.NumCopiedMacroLines = commandCount;
        MacroService.Instance.NumExecutedMacroLines = commandCount;
        MacroService.Instance.ExecuteMacroHook!.Original(UIService.Instance.raptureShellModule, macroPtr);
        MacroService.Instance.NumCopiedMacroLines = MacroStruct.numLines;
        Marshal.FreeHGlobal(macroPtr);
    }

    public List<string> Commands = [];
}
