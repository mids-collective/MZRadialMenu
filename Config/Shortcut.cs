using MZRadialMenu.Attributes;
using ImGuiNET;
using ImComponents;
namespace MZRadialMenu.Config {
    [WheelType(typeof(Shortcut))]
    public class Shortcut : BaseItem
    {
        public override void ReTree()
        {
            ImGui.PushID(this.UUID);
            if (ImGui.TreeNode(this.UUID, this.Title))
            {
                ImGui.InputText("Title", ref this.Title, 10);
                ImGui.InputText("Command", ref Command, 40);
                ImGui.TreePop();
            }
            ImGui.PopID();
        }
        public void Execute()
        {
            MZRadialMenu.Instance.ExecuteCommand(this.Command);
        }
        public override void Render(AdvRadialMenu radialMenu)
        {
            if (radialMenu.RadialMenuItem(this.Title))
            {
                this.Execute();
            }
        }
        public string Command = string.Empty;
    }
}