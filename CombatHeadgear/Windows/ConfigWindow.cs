using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CombatHeadgear.Windows;

public class ConfigWindow : Window, IDisposable
{
    public ConfigWindow() : base("CombatHeadgear Configuration###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(360, 180);
        SizeCondition = ImGuiCond.Always;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Shared.Config.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        var chatLog = Shared.Config.ShouldChatLog;
        if (ImGui.Checkbox("Chat log state change.", ref chatLog))
        {
            Shared.Config.ShouldChatLog = chatLog;
            Shared.Config.Save();
        }

        var toggleHeadgear = Shared.Config.ToggleHeadgear;
        if (ImGui.Checkbox("Toggle headgear on/off combat.", ref toggleHeadgear))
        {
            Shared.Config.ToggleHeadgear = toggleHeadgear;
            Shared.Config.Save();
        }

        var toggleVisor = Shared.Config.ToggleVisor;
        if (ImGui.Checkbox("Toggle visor on/off combat.", ref toggleVisor))
        {
            Shared.Config.ToggleVisor = toggleVisor;
            Shared.Config.Save();
        }

        var inverseToggles = Shared.Config.SetInverse;
        if (ImGui.Checkbox("Inverse toggle state. off -> on", ref inverseToggles))
        {
            Shared.Config.SetInverse = inverseToggles;
            Shared.Config.Save();
        }
        
        var delay = Shared.Config.DelayMs;
        if (ImGui.InputInt("Delay in ms", ref delay))
        {
            Shared.Config.DelayMs = delay;
            Shared.Config.Save();
        }
    }
}
