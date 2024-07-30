using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects.Enums;
using CombatHeadgear.Windows;

namespace CombatHeadgear;

public sealed class Plugin : IDalamudPlugin
{
    private IDalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private IFramework Framework { get; init; }
    private IChatGui Chat { get; init; }
    private IPluginLog PluginLog { get; init; }
    private ConfigWindow ConfigWindow { get; init; }
    private HeadgearCommandExecutor HeadgearExecutor { get; init; }
    public Configuration Configuration { get; init; }
    
    public readonly WindowSystem WindowSystem = new("CombatHeadgear");

    private const string CommandName = "/combatheadgear";
    private const string CommandAlias = "/chg";
    
    private bool _lastCombatStatus;
    private bool _isPluginDisabled;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IFramework framework,
        ISigScanner sigScanner,
        IChatGui chat,
        IPluginLog pluginLog
     )
    {
        // DI
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        Framework = framework;
        Chat = chat;
        PluginLog = pluginLog;

        // Config
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        
        // Executor
        HeadgearExecutor = new HeadgearCommandExecutor(sigScanner, PluginLog);

        // Command Handlers
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle headgear visibility in and out of combat."
        });
        CommandManager.AddHandler(CommandAlias, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle headgear visibility in and out of combat. [alias]"
        });
        
        // Hooks
        Framework.Update += OnFrameworkUpdate;
        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler(CommandAlias);
        Framework.Update -= OnFrameworkUpdate;
    }

    private void OnCommand(string command, string args)
    {
        _isPluginDisabled = !_isPluginDisabled;
        Chat.Print(_isPluginDisabled ? "Combat Headgear is now disabled." : "Combat Headgear is now enabled.");
    }

    private void DrawUi() => WindowSystem.Draw();

    private void ToggleConfigUi() => ConfigWindow.Toggle();

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (ClientState.LocalPlayer is { } player)
        {
            bool inCombat = player.StatusFlags.HasFlag(StatusFlags.InCombat);

            if (inCombat != _lastCombatStatus)
            {
                _lastCombatStatus = inCombat;
                ToggleHeadgear(inCombat);
            }
        }
    }

    private void ToggleHeadgear(bool inCombat)
    {
        if (_isPluginDisabled)
        {
            return;
        }
        
        Task.Run(() => HeadgearExecutor.ExecuteHeadgearCommand(inCombat, Configuration));

        if (Configuration.ShouldChatLog)
        {
            Chat.Print($"Toggled headgear visibility. In combat: {inCombat}");
        }
    }
}
