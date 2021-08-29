using ImGuiNET;
using System.Text;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System;

namespace MZRadialMenu
{

    public class HotkeyButton
    {
        [DllImport("user32.dll")]
        static extern int MapVirtualKey(uint uCode, uint uMapType);
        public int key = 0x0;
        [JsonIgnore]
        private bool waitingForKey = false;
        private System.Guid UUID = System.Guid.NewGuid();
        private void WaitForKey()
        {
            for (int i = 0; i < 160; i++)
            {
                if (Dalamud.Keys[i])
                {
                    waitingForKey = false;
                    key = i;
                }
            }
        }
        public void Render()
        {
            ImGui.PushID(UUID.ToString());
            if (!waitingForKey)
            {
                int chr = MapVirtualKey((uint)key, 0x2);
                var str = $"Binding: ";
                if (chr != 0)
                {
                    str += $"{Convert.ToChar(chr)}";
                }
                else
                {
                    str += "Unbound";
                }
                if (ImGui.Button(str))
                {
                    waitingForKey = true;
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
}