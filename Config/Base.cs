using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Attributes;

namespace MZRadialMenu.Config;

public delegate void PopupCallback(IMenu obj);

[JsonConverter(typeof(Converter<IMenu>))]
public interface IMenu
{
    public void RegisterCallback(PopupCallback cb);
    public bool RemoveCallback(PopupCallback cb);
    public void Render(ImComponents.Raii.IMenu rai);
    public void RenderConfig();
    public void Config(PopupCallback? CustomCallback = null);
    public string GetID();
    public void SetID(string id);
    public string GetTitle();
    public ref string GetTitleRef();
    public void SetTitle(string title);
    public void ClearID();
    public void ResetID();
}

[JsonConverter(typeof(Converter<ITemplatable>))]
public interface ITemplatable : IMenu
{
    public void RenderTemplate(TemplateObject rep, ImComponents.Raii.IMenu rai);
}

public abstract class BaseItem : IMenu
{
    private List<PopupCallback> _callbacks = [];
    public void RegisterCallback(PopupCallback cb) => _callbacks.Add(cb);
    public bool RemoveCallback(PopupCallback cb) => _callbacks.Remove(cb);
    public abstract void Render(ImComponents.Raii.IMenu rai);
    public abstract void RenderConfig();
    public void Config(PopupCallback? CustomCallback = null)
    {
        var open = ImGui.TreeNodeEx(GetID(), ImGuiTreeNodeFlags.Framed, GetTitle());
        if (_callbacks.Count != 0 || CustomCallback != null)
        {
            if (ImGui.BeginPopupContextItem())
            {
                foreach (var cb in _callbacks)
                {
                    cb(this);
                }
                CustomCallback?.Invoke(this);
                ImGui.EndPopup();
            }
        }
        if (open)
        {
            RenderConfig();
            ImGui.TreePop();
        }
    }

    public string GetID() => UUID;
    public void SetID(string id) => UUID = id;
    public virtual void ClearID() => UUID = string.Empty;
    public virtual void ResetID() => UUID = Guid.NewGuid().ToString();
    public void SetTitle(string title) => Title = title;
    public string GetTitle() => Title;
    public ref string GetTitleRef() => ref Title;
    public string UUID = Guid.NewGuid().ToString();
    public string Title = string.Empty;
}

public class Converter<T> : JsonConverter
{
    public Converter()
    {
        Renames = Registry.TypeRenames();
        Conversions = Registry.GetTypes<T>();
    }
    public Dictionary<string, string> Renames;
    public List<Type> Conversions;
    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanConvert(Type objectType) => Conversions.Contains(objectType);
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            if (Conversions.Contains(value.GetType()))
            {
                var typ = value.GetType();
                var jObj = new JObject();
                foreach (var itm in typ.GetFields())
                {
                    if (itm.FieldType.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                    {
                        jObj[itm.Name] = JToken.FromObject(itm.GetValue(value)!);
                    }
                }
                jObj["Type"] = new JValue(value.GetType().FullName);
                jObj.WriteTo(writer);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }
    }
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObj = JObject.Load(reader);
        var ti = jsonObj["Type"]!.Value<string>()!;
        if (Renames.TryGetValue(ti, out string? value))
        {
            ti = value;
        }
        jsonObj.Remove("Type");
        var obj = Activator.CreateInstance(Conversions.Where(x => x.Name == ti || x.FullName == ti).First());
        serializer.Populate(jsonObj.CreateReader(), obj!);
        return obj ?? new Menu();
    }
}

public class TemplateObject {
    public string name = string.Empty;
    public string repl = string.Empty;
    public string guid = Guid.NewGuid().ToString();
}