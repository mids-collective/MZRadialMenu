using System.Text;
using System.Text.RegularExpressions;
using MZRadialMenu.Attributes;
using MZRadialMenu.Extensions;

using Newtonsoft.Json;

using ImComponents;
using ImGuiNET;

namespace MZRadialMenu.Config;

[WheelType("Menu", false)]
public class Menu : BaseItem
{
    public bool RawRender()
    {
        bool show_buttons = true;
        ImGui.InputText("Title", ref Title, 0xF);
        for (int i = 0; i < Sublist.Count; i++)
        {
            var Item = Sublist[i];
            ImGui.PushID(Item.UUID);
            if (ImGui.Button("X"))
            {
                Sublist.RemoveAt(i);
            }

            ImGui.SameLine();
            if (ImGui.ArrowButton("##Up", ImGuiDir.Up))
            {
                var temp = Sublist[i - 1];
                Sublist.RemoveAt(i - 1);
                Sublist.Insert(i, temp);
            }
            ImGui.SameLine();
            if (ImGui.ArrowButton("##Down", ImGuiDir.Down))
            {
                var temp = Sublist[i + 1];
                Sublist.RemoveAt(i + 1);
                Sublist.Insert(i, temp);
            }
            ImGui.SameLine();
            if (ImGui.Button("Export Item"))
            {
                var cpy = Item.DeepCopy();
                cpy.UUID = string.Empty;
                var json = JsonConvert.SerializeObject(cpy);
                var exp = $"MZRM_({Convert.ToBase64String(Encoding.UTF8.GetBytes(Sublist[i].GetType().AssemblyQualifiedName!))})_({Convert.ToBase64String(Encoding.UTF8.GetBytes(json))})";
                ImGui.SetClipboardText(exp);
            }
            ImGui.SameLine();
            show_buttons &= Item.RenderConfig();
            ImGui.PopID();
        }
        if (show_buttons)
        {
            int c = 0;
            foreach (var t in Types)
            {
                if (!t.Value.Hide)
                {
                    ImGui.PushID(UUID);
                    if (ImGui.Button($"+ {t.Value.Name}"))
                    {
                        Sublist.Add((Activator.CreateInstance(t.Key) as BaseItem)!);
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
                DalamudApi.PluginLog.Info(matches.Groups[1].Captures[0].Value);
                DalamudApi.PluginLog.Info(matches.Groups[2].Captures[0].Value);
                var typ = Type.GetType(Encoding.UTF8.GetString(Convert.FromBase64String(matches.Groups[1].Captures[0].Value)));
                var obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Convert.FromBase64String(matches.Groups[2].Captures[0].Value)), typ!);
                (obj as BaseItem)!.UUID = Guid.NewGuid().ToString();
                Sublist.Add((obj as BaseItem)!);
            }
        }
        return show_buttons;
    }
    public override bool RenderConfig()
    {
        bool show_buttons = true;
        ImGui.PushID(UUID);
        if (ImGui.TreeNode(UUID, Title))
        {
            show_buttons = false;
            RawRender();
            ImGui.TreePop();
        }
        ImGui.PopID();
        return show_buttons;
    }
    public override void Render()
    {
        if (AdvRadialMenu.Instance.BeginRadialMenu(Title))
        {
            foreach (var sh in Sublist)
            {
                sh.Render();
            }
            AdvRadialMenu.Instance.EndRadialMenu();
        }
    }
    public List<BaseItem> Sublist = new();
    [JsonIgnore]
    private static Dictionary<Type, WheelTypeAttribute> Types = Registry.GetTypes<WheelTypeAttribute>().ToDictionary(x => x, y => y.GetCustomAttributes(typeof(WheelTypeAttribute), false).Select(x => x as WheelTypeAttribute).First())!;
}