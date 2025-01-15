using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using CombatHeadgear.Windows;
using Dalamud.Game;

namespace CombatHeadgear;

internal class Shared
{
    public static Configuration Config { get; set; } = null!;
    public static ConfigWindow ConfigWindow { get; set; } = null!;
    public static HeadgearCommandExecutor HeadgearExecutor { get; set; } = null!;
    
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IChatGui Chat { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
}
