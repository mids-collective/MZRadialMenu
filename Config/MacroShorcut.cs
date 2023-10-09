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
    public override void RenderConfig()
    {
        ImGui.PushID(this.UUID);
        if (ImGui.TreeNode(this.UUID, this.Title))
        {
            ImGui.InputText($"Title###{this.UUID}#Title", ref this.Title, 0xF);
            ImGui.Text($"Macro Commands");
            var combinds = String.Join('\n', this.Commands);
            ImGui.InputTextMultiline($"###{this.UUID}#Commands", ref combinds, 0x41 * 30, new Vector2(ImGui.CalcItemWidth(), 200));
            this.Commands = combinds.Split('\n').Where(x => !String.IsNullOrEmpty(x)).Take(30).ToArray();
            ImGui.TreePop();
        }
        ImGui.PopID();
    }
    public unsafe void Execute()
    {
        var macroPtr = Marshal.AllocHGlobal(ExtendedMacro.size);
        using var ExMacro = new ExtendedMacro(macroPtr, string.Empty, this.Commands);
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
        if (radialMenu.RadialMenuItem(this.Title))
        {
            this.Execute();
        }
    }
    public string[] Commands = new string[0];
}
