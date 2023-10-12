using ImGuiNET;
using Dalamud.Game.ClientState.Keys;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MZRadialMenu;

public class HotkeyButton
{
    [DllImport("user32.dll")]
    static extern int MapVirtualKey(uint uCode, uint uMapType);
    [JsonConverter(typeof(StringEnumConverter))]
    public VirtualKey key = VirtualKey.NO_KEY;
    [JsonIgnore]
    private bool waitingForKey = false;
    public Guid UUID = Guid.NewGuid();
    private void WaitForKey()
    {
        foreach (var ky in DalamudApi.Keys.GetValidVirtualKeys())
        {
            if (DalamudApi.Keys[ky])
            {
                waitingForKey = false;
                key = ky;
            }
        }
    }
    public void Render()
    {
        ImGui.PushID(UUID.ToString());
        if (!waitingForKey)
        {
            var str = $"Binding: ";
            if (key != VirtualKey.NO_KEY)
            {
                str += $"{key}";
            }
            else
            {
                str += "Unbound";
            }
            if (ImGui.Button(str))
            {
                waitingForKey = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Clear Binding"))
            {
                key = VirtualKey.NO_KEY;
            }
        }
        else
        {
            WaitForKey();
            var buf = "Waiting for Key";
            ImGui.Text(buf);
        }
        ImGui.PopID();
    }
}