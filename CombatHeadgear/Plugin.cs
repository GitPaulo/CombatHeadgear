using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects.Enums;
using CombatHeadgear.Windows;

namespace CombatHeadgear;

public sealed class Plugin : IDalamudPlugin
{
    private const string PluginName = "CombatHeadgear";
    private const string PrimaryCommand = "/combatheadgear";
    private const string CommandShortcut = "/chg";

    private readonly WindowSystem windowSystem = new(PluginName);

    private bool lastCombatStatus;
    private bool isPluginDisabled;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Shared>();

        Shared.Config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        InitWindows();
        InitCommands();
        InitServices();
        InitHooks();

        Shared.Log.Information($"Loaded {Shared.PluginInterface.Manifest.Name}");
    }

    private void InitWindows()
    {
        Shared.ConfigWindow = new ConfigWindow();
        windowSystem.AddWindow(Shared.ConfigWindow);
    }

    private void InitCommands()
    {
        Shared.CommandManager.AddHandler(PrimaryCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle headgear visibility in and out of combat."
        });
        Shared.CommandManager.AddHandler(CommandShortcut, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle headgear visibility in and out of combat. [alias]"
        });
    }

    private void InitServices()
    {
        Shared.HeadgearExecutor = new HeadgearCommandExecutor();
    }

    private void InitHooks()
    {
        Shared.Framework.Update += OnFrameworkUpdate;
        Shared.PluginInterface.UiBuilder.Draw += DrawUi;
        Shared.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        windowSystem.RemoveAllWindows();
        Shared.ConfigWindow.Dispose();

        Shared.CommandManager.RemoveHandler(PrimaryCommand);
        Shared.CommandManager.RemoveHandler(CommandShortcut);
        Shared.Framework.Update -= OnFrameworkUpdate;
    }

    private void OnCommand(string command, string args)
    {
        isPluginDisabled = !isPluginDisabled;
        Shared.Chat.Print(isPluginDisabled ? "Combat Headgear is now disabled." : "Combat Headgear is now enabled.");
    }

    private void DrawUi() => windowSystem.Draw();
    private void ToggleConfigUi() => Shared.ConfigWindow.Toggle();

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (Shared.ClientState.LocalPlayer is { } player)
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
        if (isPluginDisabled)
        {
            return;
        }

        Task.Run(() => Shared.HeadgearExecutor.ExecuteHeadgearCommand(inCombat));

        if (Shared.Config.ShouldChatLog)
        {
            Shared.Chat.Print($"Toggled headgear visibility. In combat: {inCombat}");
        }
    }
}
