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

    // the below exist just to make saving less cumbersome
    [NonSerialized] private DalamudPluginInterface? PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}