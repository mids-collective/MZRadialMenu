using System;
using System.Collections.Generic;
using System.Linq;
using ImComponents;
using MZRadialMenu.Attributes;
using ImGuiNET;
using Newtonsoft.Json;
namespace MZRadialMenu.Config
{
    [WheelType("Menu", false)]
    public class Menu : BaseItem
    {
        public void RawRender()
        {
            ImGui.InputText("Title", ref this.Title, 0xF);
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
            int c = 0;
            foreach (var t in Types)
            {
                if (!t.Value.Hide)
                {
                    ImGui.PushID(this.UUID);
                    if (ImGui.Button($"+ {t.Value.Name}"))
                    {
                        this.Sublist.Add((Activator.CreateInstance(t.Key) as BaseItem)!);
                    }
                    if (++c != Types.Count - 1)
                    {
                        ImGui.SameLine();
                    }
                    ImGui.PopID();
                }
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
        [JsonIgnore]
        private static Dictionary<Type, WheelTypeAttribute> Types = Registry.GetTypes<WheelTypeAttribute>().ToDictionary(x => x, y => y.GetCustomAttributes(typeof(WheelTypeAttribute), false).Select(x => x as WheelTypeAttribute).First())!;
    }
}