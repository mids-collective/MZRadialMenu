using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ImComponents.Raii;
using ImGuiNET;
using Newtonsoft.Json;
using Plugin;
using Plugin.Attributes;

namespace MZRadialMenu.Config;

public partial class Menu : BaseItem, ITemplatable
{
    public Menu() : base()
    {
        RegisterCallback(MenuPopup);
        RegisterCallback(AddItemPopup);
    }
    private int current_item;
    public void AddItemPopup(IBaseItem ti)
    {
        ImGui.Text("Item to add");
        ImGui.ListBox($"##{ti.GetID()}", ref current_item, [.. ItemNames], ItemNames.Count);
        if (ImGui.Button("Add Item"))
        {
            var item = Activator.CreateInstance(Types[current_item]);
            Sublist.Add((item as IBaseItem)!);
        }
    }
    public void MenuPopup(IBaseItem ti)
    {
        ImGui.InputText($"Title##{ti.GetID()}", ref Title, 0xF);
        if (ImGui.Button($"Import from clipboard##{ti.GetID()}"))
        {
            var clip = ImGui.GetClipboardText();
            var matches = Constants.mzri.Matches(clip).ToHashSet();
            foreach (var match in matches)
            {
                var import = match.Groups["import"].Value;
                var obj = JsonConvert.DeserializeObject<IBaseItem>(Encoding.UTF8.GetString(Convert.FromBase64String(import)));
                if (obj != null)
                {
                    obj.ResetID();
                    Sublist.Add(obj);
                }
            }
        }
    }
    public void GenericPopup(IBaseItem ti)
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
    public override void Render(IMenu im)
    {
        using var raii = im.Menu(Title);
        if (raii.open)
        {
            foreach (var sh in Sublist)
            {
                sh.Render(raii);
            }
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

    public void RenderTemplate(TemplateObject rep, IMenu im)
    {
        Render(im);
    }

    public List<IBaseItem> Sublist = [];
    [JsonIgnore]
    private static List<Type> Types => Registry.GetTypes<IBaseItem>();
    [JsonIgnore]
    private static List<string> ItemNames => Types.Where(x => x.GetCustomAttribute<HiddenAttribute>() == null).Select(x => $"{x.GetCustomAttribute<DisplayNameAttribute>()?.Name ?? x.Name}").ToList();
}