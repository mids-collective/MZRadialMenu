using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;
using MZRadialMenu.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ImComponents;
namespace MZRadialMenu.Config
{
    [JsonConverter(typeof(Converter))]
    public abstract class BaseItem
    {
        public abstract void ReTree();
        public abstract void Render(AdvRadialMenu radialMenu);
        public string UUID = System.Guid.NewGuid().ToString();
        public string Title = string.Empty;
    }
    public class Converter : JsonConverter
    {
        static IEnumerable<Type> GetTypesWithHelpAttribute(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(WheelTypeAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }
        public Converter()
        {
            var Types = AppDomain.CurrentDomain.GetAssemblies().AsParallel().SelectMany(x => x.GetTypes());
            Conversions = Types.Where(attr => attr.GetCustomAttributes(typeof(WheelTypeAttribute), true).Length > 0).ToList();
        }
        public List<Type> Conversions;
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(System.Type objectType)
        {
            return Conversions.Contains(objectType);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jObj = new JObject();
            var typ = value.GetType();
            foreach (var field in typ.GetFields())
            {
                if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                {
                    continue;
                }
                var tmp = new JTokenWriter();
                serializer.Serialize(tmp, field.GetValue(value), field.FieldType);
                jObj.Add(new JProperty(field.Name, tmp.Token));
            }
            jObj.Add(new JProperty("Type", new JValue(value.GetType().FullName)));
            jObj.WriteTo(writer);
        }
        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObj = JObject.Load(reader);
            var obj = Activator.CreateInstance(Conversions.Where(x => x.FullName == (string)jsonObj["Type"].Value<string>()).First());
            jsonObj.Remove("Type");
            var typ = obj.GetType();
            foreach (var vobj in jsonObj)
            {
                var f = typ.GetField(vobj.Key);
                if (f.FieldType.IsClass && f.FieldType != typeof(string))
                {
                    var tmp = Activator.CreateInstance(f.FieldType);
                    serializer.Populate(vobj.Value.CreateReader(), tmp);
                    f.SetValue(obj, tmp);
                }
                else
                {
                    switch (vobj.Value.Type)
                    {
                        case JTokenType.String:
                            f.SetValue(obj, (string)vobj.Value);
                            break;
                        case JTokenType.Boolean:
                            f.SetValue(obj, (bool)vobj.Value);
                            break;
                    }
                }
            }
            return (object)obj;
        }
    }
}