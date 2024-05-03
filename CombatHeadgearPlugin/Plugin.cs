using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Logging;
using CombatHeadgearPlugin.Windows;

namespace CombatHeadgearPlugin;

public sealed class Plugin : IDalamudPlugin
{
    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    const string CommandName = "/combatheadgear";
    
    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private IFramework Framework { get; init; }
    private IChatGui Chat { get; init; }
    private ConfigWindow ConfigWindow { get; init; }
    private HeadgearCommandExecutor HeadgearExecutor { get; init; }
    public Configuration Configuration { get; init; }
    
    private bool lastCombatStatus;
    private bool isPluginDisabled;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] IClientState clientState,
        [RequiredVersion("1.0")] IFramework framework,
        [RequiredVersion("1.0")] ISigScanner sigScanner,
        IChatGui chat)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        Framework = framework;
        Chat = chat;
        
        HeadgearExecutor = new HeadgearCommandExecutor(sigScanner);

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle headgear visibility in and out of combat."
        });

        Framework.Update += OnFrameworkUpdate;
        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        Framework.Update -= OnFrameworkUpdate;
    }

    private void OnCommand(string command, string args)
    {
        isPluginDisabled = !isPluginDisabled;
        PluginLog.Log(isPluginDisabled ? "Plugin is now disabled." : "Plugin is now enabled.");
    }

    private void DrawUi() => WindowSystem.Draw();

    public void ToggleConfigUi() => ConfigWindow.Toggle();

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (ClientState.LocalPlayer is { } player)
        {
            bool inCombat = player.StatusFlags.HasFlag(StatusFlags.InCombat);

            if (inCombat != lastCombatStatus)
            {
                lastCombatStatus = inCombat;
                ToggleHeadgear(inCombat);
            }
        }
    }

    private void ToggleHeadgear(bool inCombat)
    {
        HeadgearExecutor.ExecuteHeadgearCommand(inCombat);
        PluginLog.Debug($"Toggled headgear visibility. In combat: {inCombat}");
        Chat.Print($"Toggled headgear visibility. In combat: {inCombat}");
    }
}
