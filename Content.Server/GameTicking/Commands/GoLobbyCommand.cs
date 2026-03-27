using Content.Server.Administration;
using Content.Server.GameTicking.Presets;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class GoLobbyCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly IGameTiming _time = default!; // L5

        public override string Command => "golobby";

        // L5 - add confirm
        public string Help => $"Usage: {Command} [confirm] / {Command} <preset> [confirm]";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            GamePresetPrototype? preset = null;
            var presetName = string.Join(" ", args);
            // Begin L5 changes - add confirm
            var confirm = presetName.EndsWith("confirm");
            if (confirm)
                presetName = presetName[..^"confirm".Length].TrimEnd();
            else if (_time.RealTime > TimeSpan.FromMinutes(30))  // starcup: FromHours(1) -> FromMinutes(30)
            {
                shell.WriteLine($"Add 'confirm' to the command to really end the round and go back to the lobby.");
                return;
            }
            // End L5 changes

            if (presetName.Length > 0) // L5 - was args.Length
            {
                if (!_gameTicker.TryFindGamePreset(presetName, out preset))
                {
                    shell.WriteLine(Loc.GetString($"cmd-forcepreset-no-preset-found", ("preset", presetName)));
                    return;
                }
            }

            _configManager.SetCVar(CCVars.GameLobbyEnabled, true);

            _gameTicker.RestartRound();

            if (preset != null)
                _gameTicker.SetGamePreset(preset);

            shell.WriteLine(Loc.GetString(preset == null ? "cmd-golobby-success" : "cmd-golobby-success-with-preset", ("preset", presetName)));
        }
    }
}
