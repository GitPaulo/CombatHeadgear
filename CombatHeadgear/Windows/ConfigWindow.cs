using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CombatHeadgear.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration _configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("CombatHeadgear Configuration###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(380, 205);
        SizeCondition = ImGuiCond.Always;

        _configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (_configuration.IsConfigWindowMovable)
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
        var movable = _configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            _configuration.IsConfigWindowMovable = movable;
            _configuration.Save();
        }

        var chatLog = _configuration.ShouldChatLog;
        if (ImGui.Checkbox("Should chat log state change.", ref chatLog))
        {
            _configuration.ShouldChatLog = chatLog;
            _configuration.Save();
        }


        var toggleHeadgear = _configuration.ToggleHeadgear;
        if (ImGui.Checkbox("Toggle headgear on/off combat.", ref toggleHeadgear))
        {
            _configuration.ToggleHeadgear = toggleHeadgear;
            _configuration.Save();
        }

        var toggleVisor = _configuration.ToggleVisor;
        if (ImGui.Checkbox("Toggle visor on/off combat.", ref toggleVisor))
        {
            _configuration.ToggleVisor = toggleVisor;
            _configuration.Save();
        }

        var inverseToggles = _configuration.SetInverse;
        if (ImGui.Checkbox("Inverse toggle state. off -> on", ref inverseToggles))
        {
            _configuration.SetInverse = inverseToggles;
            _configuration.Save();
        }
        
        var delay = _configuration.DelayMs;
        if (ImGui.InputInt("Delay in milliseconds", ref delay))
        {
            _configuration.DelayMs = delay;
            _configuration.Save();
        }
    }
}