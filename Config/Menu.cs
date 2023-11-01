using System.Text;
using Plugin;
using Newtonsoft.Json;
using ImComponents;
using ImGuiNET;
using System.Text.RegularExpressions;
using Plugin.Attributes;
using System.Reflection;

namespace MZRadialMenu.Config;

public class Menu : BaseItem
{
    public Menu() : base()
    {
        RegisterCallback(MenuPopup);
        RegisterCallback(AddItemPopup);
    }
    private int current_item;
    public void AddItemPopup(IMenu ti)
    {
        ImGui.Text("Item to add");
        ImGui.ListBox($"##{ti.GetID()}", ref current_item, item_names.ToArray(), item_names.Count);
        if (ImGui.Button("Add Item"))
        {
            var item = Activator.CreateInstance(types[current_item]);
            Sublist.Add((item as IMenu)!);
        }
    }
    public void MenuPopup(IMenu ti)
    {
        ImGui.InputText($"Title##{ti.GetID()}", ref Title, 0xF);
        if (ImGui.Button($"Import from clipboard##{ti.GetID()}"))
        {
            var clip = ImGui.GetClipboardText();
            var regex = new Regex(@"MZRI_\((?<import>.*)\)");
            var matches = regex.Matches(clip).ToHashSet();
            foreach (var match in matches)
            {
                var import = match.Groups["import"].Value;
                var obj = JsonConvert.DeserializeObject<IMenu>(Encoding.UTF8.GetString(Convert.FromBase64String(import)));
                if (obj != null)
                {
                    obj.ResetID();
                    Sublist.Add(obj);
                }
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
        if (AdvRadialMenu.Instance.BeginRadialMenu(GetTitle()))
        {
            foreach (var sh in Sublist)
            {
                sh.Render();
            }
            AdvRadialMenu.Instance.EndRadialMenu();
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
    public List<IMenu> Sublist = new();
    [JsonIgnore]
    private List<Type> types => Registry.GetTypes<IMenu>();
    [JsonIgnore]
    private List<string> item_names => types.Where(x => x.GetCustomAttribute<HiddenAttribute>() == null).Select(x => $"{x.GetCustomAttribute<DisplayNameAttribute>()?.Name ?? x.Name}").ToList();
}