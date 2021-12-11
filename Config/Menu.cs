using System.Collections.Generic;
using ImComponents;
using MZRadialMenu.Attributes;
using ImGuiNET;
namespace MZRadialMenu.Config {
    [WheelType(typeof(Menu))]
    public class Menu : BaseItem
    {
        public void RawRender()
        {
            ImGui.InputText("Title", ref this.Title, 20);
            for (int i = 0; i < this.Sublist.Count; i++)
            {
                ImGui.PushID(this.Sublist[i].UUID);
                if (ImGui.Button("X"))
                {
                    this.Sublist.RemoveAt(i);
                }
                else
                {
                    ImGui.SameLine();
                    this.Sublist[i].ReTree();
                }
                ImGui.PopID();
            }
            if (ImGui.Button("+ Menu"))
            {
                this.Sublist.Add(new Menu());
            }
            ImGui.SameLine();
            if (ImGui.Button("+ Shortcut"))
            {
                this.Sublist.Add(new Shortcut());
            }
            ImGui.SameLine();
            if (ImGui.Button("+ Teleport"))
            {
                this.Sublist.Add(new Teleport());
            }
            ImGui.SameLine();
            if (ImGui.Button("+ Job"))
            {
                this.Sublist.Add(new Job());
            }
        }
        public override void ReTree()
        {
            ImGui.PushID(this.UUID);
            if (ImGui.TreeNode(this.UUID, this.Title))
            {
                this.RawRender();
                ImGui.TreePop();
            }
            ImGui.PopID();
        }
        public override void Render(AdvRadialMenu radialMenu)
        {
            if (radialMenu.BeginRadialMenu(this.Title))
            {
                foreach (var sh in this.Sublist)
                {
                    sh.Render(radialMenu);
                }
                radialMenu.EndRadialMenu();
            }
        }
        public List<BaseItem> Sublist = new();
    }
}