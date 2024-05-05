using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace CombatHeadgearPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool IsConfigWindowMovable { get; set; } = true;
    public bool ShouldChatLog { get; set; } = true;
    public bool ToggleHeadgear { get; set; } = true;
    public bool ToggleVisor { get; set; } = false;
    public bool SetInverse { get; set; } = false;
    public int DelayMs { get; set; } = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized] private DalamudPluginInterface? _pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
}