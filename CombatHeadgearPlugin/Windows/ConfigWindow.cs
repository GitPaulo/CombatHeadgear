using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CombatHeadgearPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("CombatHeadgear Configuration###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(300, 400);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose()
    {
    }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
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
        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }

        var chatLog = Configuration.ShouldChatLog;
        if (ImGui.Checkbox("Should chat log state change.", ref chatLog))
        {
            Configuration.ShouldChatLog = chatLog;
            Configuration.Save();
        }


        var toggleHeadgear = Configuration.ToggleHeadgear;
        if (ImGui.Checkbox("Toggle headgear on/off combat.", ref toggleHeadgear))
        {
            Configuration.ToggleHeadgear = toggleHeadgear;
            Configuration.Save();
        }

        var toggleVisor = Configuration.ToggleVisor;
        if (ImGui.Checkbox("Toggle visor on/off combat.", ref toggleVisor))
        {
            Configuration.ToggleVisor = toggleVisor;
            Configuration.Save();
        }


        var inverseToggles = Configuration.SetInverse;
        if (ImGui.Checkbox("Inverse toggle state. off -> on", ref inverseToggles))
        {
            Configuration.SetInverse = inverseToggles;
            Configuration.Save();
        }
    }
}