using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

using ImComponents;
using ImGuiNET;

using MZRadialMenu.Attributes;
using MZRadialMenu.Structures;

namespace MZRadialMenu.Config;

//Virtual macros?
//Need to limit text output macros
[WheelType("Macro", false)]
public class MacroShortcut : BaseItem
{
    public override bool RenderConfig()
    {
        ImGui.PushID(UUID);
        if (ImGui.TreeNode(UUID, Title))
        {
            ImGui.InputText($"Title###{UUID}#Title", ref Title, 0xF);
            ImGui.Text($"Macro Commands");
            var combinds = String.Join('\n', Commands);
            ImGui.InputTextMultiline($"###{UUID}#Commands", ref combinds, 0x41 * 30, new Vector2(ImGui.CalcItemWidth(), 200));
            Commands = combinds.Split('\n').Where(x => !String.IsNullOrEmpty(x)).Take(30).ToArray();
            ImGui.TreePop();
        }
        ImGui.PopID();
        return true;
    }
    public unsafe void Execute()
    {
        var macroPtr = Marshal.AllocHGlobal(ExtendedMacro.size);
        using var ExMacro = new ExtendedMacro(macroPtr, string.Empty, Commands);
        Marshal.StructureToPtr(ExMacro, macroPtr, false);
        var commandCount = (byte)Math.Max(Macro.numLines, Commands.Length);
        MZRadialMenu.NumCopiedMacroLines = commandCount;
        MZRadialMenu.NumExecutedMacroLines = commandCount;
        MZRadialMenu.ExecuteMacroHook!.Original(MZRadialMenu.raptureShellModule, macroPtr);
        MZRadialMenu.NumCopiedMacroLines = Macro.numLines;
        Marshal.FreeHGlobal(macroPtr);
    }
    public override void Render(AdvRadialMenu radialMenu)
    {
        if (radialMenu.RadialMenuItem(Title))
        {
            Execute();
        }
    }
    public string[] Commands = new string[0];
}
