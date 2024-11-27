using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Dalamud.Hooking;
using Dalamud;
using Plugin.Structures;

namespace Plugin.Services;

public unsafe sealed class MacroService : IService<MacroService>
{
    public static MacroService Instance => Service<MacroService>.Instance;
    public static void Execute(int id) => Instance.ExecuteMacro(id);
    public void ExecuteMacro(int macro)
    {
        if (macro < 100)
        {
            fixed (void* ptr = &UIService.Instance.raptureMacroModule->Individual[macro])
                ExecuteMacroHook!.Original(UIService.Instance.raptureShellModule, (nint)ptr);
        }
        else
        {
            fixed (void* ptr = &UIService.Instance.raptureMacroModule->Shared[macro - 100])
                ExecuteMacroHook!.Original(UIService.Instance.raptureShellModule, (nint)ptr);
        }
    }
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
    private MacroService()
    {
        ExecuteMacroHook = DalamudApi.GameInteropProvider.HookFromSignature<ExecuteMacroDelegate>(SigService.GetSig("Macro"), ExecuteMacroDetour);
        numCopiedMacroLinesPtr = DalamudApi.SigScanner.ScanText(SigService.GetSig("CopiedMacroLines")) + 0x5;
        numExecutedMacroLinesPtr = DalamudApi.SigScanner.ScanText(SigService.GetSig("ExecutedMacroLines")) + 0x3;
        ExecuteMacroHook!.Enable();
    }

    private void ExecuteMacroDetour(RaptureShellModule* raptureShellModule, nint macro)
    {
        NumCopiedMacroLines = MacroStruct.numLines;
        NumExecutedMacroLines = MacroStruct.numLines;
        ExecuteMacroHook!.Original(raptureShellModule, macro);
    }
    public void Dispose()
    {
        NumCopiedMacroLines = 15;
        NumExecutedMacroLines = 15;
        ExecuteMacroHook?.Dispose();
    }
}