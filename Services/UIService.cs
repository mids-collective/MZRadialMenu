using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace Plugin.Services;

public unsafe sealed class UIService : IService<UIService>
{
    public static UIService Instance => Service<UIService>.Instance;
    public UIModule* uiModule => Framework.Instance()->GetUIModule();
    public RaptureShellModule* raptureShellModule => uiModule->GetRaptureShellModule();
    public RaptureMacroModule* raptureMacroModule => uiModule->GetRaptureMacroModule();
    private AgentModule* agentModule => uiModule->GetAgentModule();
    public nint GetAgentByInternalID(AgentId id) => (nint)agentModule->GetAgentByInternalId(id);
    public bool IsGameTextInputActive => uiModule->GetRaptureAtkModule()->AtkModule.IsTextInputActive();
    public AgentInventoryContext* agentInventoryContext => (AgentInventoryContext*)uiModule->GetAgentModule()->GetAgentByInternalId(AgentId.InventoryContext);
    private UIService()
    {
    }
    public void Dispose()
    {

    }
}