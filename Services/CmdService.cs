using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using MZRadialMenu.Structures;

namespace MZRadialMenu.Services;
public unsafe sealed class CmdService : IDisposable
{
    public static CmdService Instance => Service<CmdService>.Instance;
    private const string ChatBoxSig = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9";
    private CmdService() { 
        ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(DalamudApi.SigScanner.ScanModule(ChatBoxSig));
    }

    public void ExecuteCommand(string command)
    {
        if (command.StartsWith("//"))
        {
            command = command[2..].ToLower();
            switch (command[0])
            {
                case 'i':
                    ItemService.Instance.UseItem(command[2..]);
                    break;
                case 'm':
                    int.TryParse(command[1..], out int val);
                    if (val is >= 0 and < 200)
                    {
                        MacroService.Instance.ExecuteMacroHook!.Original(UIService.Instance.raptureShellModule, (nint)UIService.Instance.raptureMacroModule + 0x58 + (Macro.size * val));
                    }
                    break;
            }
            return;
        }
        var stringPtr = nint.Zero;
        try
        {
            stringPtr = Marshal.AllocHGlobal(UTF8String.size);
            using var str = new UTF8String(stringPtr, command);
            Marshal.StructureToPtr(str, stringPtr, false);
            ProcessChatBox!(UIService.Instance.uiModule, stringPtr, nint.Zero, 0);
        }
        catch
        {
            DalamudApi.PluginLog.Error("Error with injecting command");
        }
        Marshal.FreeHGlobal(stringPtr);
    }

    // Command Execution
    private delegate void ProcessChatBoxDelegate(UIModule* uiModule, nint message, nint unused, byte a4);
    private ProcessChatBoxDelegate? ProcessChatBox;
    public void Dispose()
    {
    }
}