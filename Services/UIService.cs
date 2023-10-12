using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace MZRadialMenu.Services;
public unsafe sealed class UIService : IDisposable
{
    public static UIService Instance => Service<UIService>.Instance;
    public UIModule* uiModule;
    public RaptureShellModule* raptureShellModule;
    public RaptureMacroModule* raptureMacroModule;
    private AgentModule* agentModule;
    private UIService() { 
        uiModule = Framework.Instance()->GetUiModule();
        agentModule = uiModule->GetAgentModule();

        raptureShellModule = uiModule->GetRaptureShellModule();
        raptureMacroModule = uiModule->GetRaptureMacroModule();
    }
    public nint GetAgentByInternalID(AgentId id) => (nint)agentModule->GetAgentByInternalId(id);
    public bool IsGameTextInputActive => uiModule->GetRaptureAtkModule()->AtkModule.IsTextInputActive();
    public void Dispose() {

    }
}