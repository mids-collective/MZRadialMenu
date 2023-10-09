using System;
using System.Collections.Generic;
using System.Linq;
using ImComponents;
using MZRadialMenu.Attributes;
using ImGuiNET;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace MZRadialMenu.Config;

[WheelType("Menu", false)]
public class Menu : BaseItem
{
    public bool RawRender()
    {
        bool show_buttons = true;
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
                if (ImGui.ArrowButton("##Up", ImGuiDir.Up))
                {
                    var temp = this.Sublist[i - 1];
                    this.Sublist.RemoveAt(i - 1);
                    this.Sublist.Insert(i, temp);
                    continue;
                }
                ImGui.SameLine();
                if (ImGui.ArrowButton("##Down", ImGuiDir.Down))
                {
                    var temp = this.Sublist[i + 1];
                    this.Sublist.RemoveAt(i + 1);
                    this.Sublist.Insert(i, temp);
                    continue;
                }
                ImGui.SameLine();
                if (ImGui.Button("Export Item"))
                {
                    var json = JsonConvert.SerializeObject(this.Sublist[i]);
                    var exp = $"MZRM_({Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Sublist[i].GetType().AssemblyQualifiedName!))})_({Convert.ToBase64String(Encoding.UTF8.GetBytes(json))})";
                    ImGui.SetClipboardText(exp);
                }
                ImGui.SameLine();
                show_buttons &= this.Sublist[i].RenderConfig();
            }
            ImGui.PopID();
        }
        if (show_buttons)
        {
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
            ImGui.SameLine();
            if (ImGui.Button("Import Item"))
            {
                var clip = ImGui.GetClipboardText();
                var regex = new Regex(@"MZRM_\((.*)\)_\((.*)\)");
                var matches = regex.Match(clip);
                Dalamud.PluginLog.Info(matches.Groups[1].Captures[0].Value);
                Dalamud.PluginLog.Info(matches.Groups[2].Captures[0].Value);
                var typ = Type.GetType(Encoding.UTF8.GetString(Convert.FromBase64String(matches.Groups[1].Captures[0].Value)));
                var obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Convert.FromBase64String(matches.Groups[2].Captures[0].Value)), typ!);
                (obj as BaseItem)!.UUID = System.Guid.NewGuid().ToString();
                this.Sublist.Add((obj as BaseItem)!);
            }
        }
        return show_buttons;
    }
    public override bool RenderConfig()
    {
        bool show_buttons = true;
        ImGui.PushID(this.UUID);
        if (ImGui.TreeNode(this.UUID, this.Title))
        {
            show_buttons = false;
            this.RawRender();
            ImGui.TreePop();
        }
        ImGui.PopID();
        return show_buttons;
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