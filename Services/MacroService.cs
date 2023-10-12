using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud;
using MZRadialMenu.Structures;

namespace MZRadialMenu.Services;

public unsafe sealed class MacroService : IDisposable
{
    public static MacroService Instance => Service<MacroService>.Instance;
    private const string macroSig = "E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 4D 28";
    // Macro Execution
    public delegate void ExecuteMacroDelegate(RaptureShellModule* raptureShellModule, nint macro);
    public Hook<ExecuteMacroDelegate>? ExecuteMacroHook;
    private nint numCopiedMacroLinesPtr = nint.Zero;
    public byte NumCopiedMacroLines
    {
        get => *(byte*)numCopiedMacroLinesPtr;
        set
        {
            if (numCopiedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numCopiedMacroLinesPtr, new[] { value });
        }
    }

    private nint numExecutedMacroLinesPtr = nint.Zero;
    public byte NumExecutedMacroLines
    {
        get => *(byte*)numExecutedMacroLinesPtr;
        set
        {
            if (numExecutedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numExecutedMacroLinesPtr, new[] { value });
        }
    }
    private MacroService() { }
    public void Initialize()
    {
        DalamudApi.GameInteropProvider.InitializeFromAttributes(this);
        ExecuteMacroHook = DalamudApi.GameInteropProvider.HookFromSignature<ExecuteMacroDelegate>(macroSig, ExecuteMacroDetour);
        numCopiedMacroLinesPtr = DalamudApi.SigScanner.ScanText("49 8D 5E 70 BF ?? 00 00 00") + 0x5;
        numExecutedMacroLinesPtr = DalamudApi.SigScanner.ScanText("41 83 F8 ?? 0F 8D ?? ?? ?? ?? 49 6B C8 68") + 0x3;

        ExecuteMacroHook!.Enable();
    }

    private void ExecuteMacroDetour(RaptureShellModule* raptureShellModule, nint macro)
    {
        NumCopiedMacroLines = Macro.numLines;
        NumExecutedMacroLines = Macro.numLines;
        ExecuteMacroHook!.Original(raptureShellModule, macro);
    }
    public void Dispose()
    {
        NumCopiedMacroLines = 15;
        NumExecutedMacroLines = 15;
        ExecuteMacroHook?.Dispose();
    }
}