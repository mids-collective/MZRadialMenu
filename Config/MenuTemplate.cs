using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ImComponents;
using ImGuiNET;
using Newtonsoft.Json;
using Plugin;
using Plugin.Attributes;

namespace MZRadialMenu.Config;

[DisplayName("Template Menu")]
public class MenuTemplate : BaseItem
{
    public MenuTemplate() : base()
    {
        RegisterCallback(MenuPopup);
        RegisterCallback(AddItemPopup);
    }
    private int current_item;
    public void AddItemPopup(IMenu ti)
    {
        ImGui.Text("Item to add");
        ImGui.ListBox($"##{ti.GetID()}", ref current_item, [.. ItemNames], ItemNames.Count);
        if (ImGui.Button("Add Item"))
        {
            var item = Activator.CreateInstance(Types[current_item]);
            Sublist.Add((item as ITemplatable)!);
        }
    }
    public void MenuPopup(IMenu ti)
    {
        ImGui.InputText($"Title##{ti.GetID()}", ref Title, 0xF);
        if (ImGui.Button($"Import from clipboard##{ti.GetID()}"))
        {
            var clip = ImGui.GetClipboardText();
            var matches = Constants.mzri.Matches(clip).ToHashSet();
            foreach (var match in matches)
            {
                var import = match.Groups["import"].Value;
                var obj = JsonConvert.DeserializeObject<ITemplatable>(Encoding.UTF8.GetString(Convert.FromBase64String(import)));
                if (obj != null)
                {
                    obj.ResetID();
                    Sublist.Add(obj);
                }
            }
        }
        if (ImGui.Button($"+ Template##{GetID()}"))
        {
            templates.Add(new TemplateObject());
        }
        for (int i = 0; i < templates.Count(); i++)
        {
            var temp = templates[i];
            ImGui.InputText($"##{templates[i].guid}", ref temp.name, 0x20);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Name of the template");
            }
            ImGui.SameLine();
            ImGui.InputText($"##{templates[i].guid}", ref temp.repl, 0x20);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Replaces <tmpl> in any following executing lines (Macros/Shortcuts/etc)");
            }
            ImGui.SameLine();
            if (ImGui.Button($"X##{templates[i].guid}"))
            {
                templates.RemoveAt(i);
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Delete this template");
            }
        }
    }
    public void GenericPopup(IMenu ti)
    {
        var i = Sublist.FindIndex(x => x.GetID() == ti.GetID());
        if (ImGui.Button($"Export to clipboard##{ti.GetID()}"))
        {
            var cpy = ti.DeepCopy();
            cpy.ClearID();
            var json = JsonConvert.SerializeObject(cpy);
            var exp = $"MZRI_({Convert.ToBase64String(Encoding.UTF8.GetBytes(json))})";
            ImGui.SetClipboardText(exp);
        }
        ImGui.Separator();
        if (ImGui.ArrowButton($"##Up.{ti.GetID()}", ImGuiDir.Up))
        {
            var temp = Sublist[i - 1];
            Sublist.RemoveAt(i - 1);
            Sublist.Insert(i, temp);
        }
        ImGui.SameLine();
        if (ImGui.ArrowButton($"##Down.{ti.GetID()}", ImGuiDir.Down))
        {
            var temp = Sublist[i + 1];
            Sublist.RemoveAt(i + 1);
            Sublist.Insert(i, temp);
        }
        ImGui.Separator();
        if (ImGui.Button($"Delete {ti.GetTitle()}"))
        {
            Sublist.RemoveAt(i);
            ImGui.CloseCurrentPopup();
        }
    }
    public override void RenderConfig()
    {
        foreach (var item in Sublist)
        {
            item.Config(GenericPopup);
        }
    }
    public override void Render()
    {
        if (RadialMenu.Instance.BeginRadialMenu(GetTitle()))
        {
            foreach (var temp in templates)
            {
                if (RadialMenu.Instance.BeginRadialMenu(temp.name))
                {
                    foreach (var sh in Sublist)
                    {
                        sh.RenderTemplate(temp);
                    }
                    RadialMenu.Instance.EndRadialMenu();
                }
            }
            RadialMenu.Instance.EndRadialMenu();
        }
    }
    public override void ClearID()
    {
        SetID(string.Empty);
        foreach (var itm in Sublist)
        {
            itm.ClearID();
        }
    }
    public override void ResetID()
    {
        SetID(Guid.NewGuid().ToString());
        foreach (var itm in Sublist)
        {
            itm.ResetID();
        }
    }
    public List<TemplateObject> templates = [];
    public List<ITemplatable> Sublist = [];
    [JsonIgnore]
    private static List<Type> Types => Registry.GetTypes<ITemplatable>();
    [JsonIgnore]
    private static List<string> ItemNames => Types.Where(x => x.GetCustomAttribute<HiddenAttribute>() == null).Select(x => $"{x.GetCustomAttribute<DisplayNameAttribute>()?.Name ?? x.Name}").ToList();
}