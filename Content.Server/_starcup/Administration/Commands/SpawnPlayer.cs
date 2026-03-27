using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SpawnPlayer : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly StationSpawningSystem _spawningSystem = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public override string Command => "spawnplayer";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        // Argument 1: Player, required
        if (args.Length < 1)
        {
            shell.WriteError(Loc.GetString("shell-need-minimum-one-argument"));
            return;
        }

        if (!_playerManager.TryGetSessionByUsername(args[0], out var player))
        {
            shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        // Targeted player must be attached to an entity
        if (player.AttachedEntity is not { })
        {
            shell.WriteError(Loc.GetString("shell-must-be-attached-to-entity"));
            return;
        }

        // Must have a mind
        if (!_mindSystem.TryGetMind(player, out var mindEnt, out _))
        {
            shell.WriteError(Loc.GetString("shell-entity-is-not-mob"));
            return;
        }

        // Argument 2: Job, Optional
        var jobName = args.Length > 1 ? args[1] : "Passenger"; // just default to passenger

        if (!_prototypeManager.HasIndex<JobPrototype>(jobName))
        {
            shell.WriteError(Loc.GetString("cmd-spawncharacter-error-job"));
            return;
        }

        // Argument 3: apply traits, Optional
        var doTraits = true;
        if (args.Length > 2 && !bool.TryParse(args[2], out doTraits))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-boolean"));
            return;
        }

        // Argument 4: transfer mind, Optional
        var transferMind = true;
        if (args.Length > 3 && !bool.TryParse(args[3], out transferMind))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-boolean"));
            return;
        }

        // Coordinates are either the admin entity or a spawn point if we called from the server
        var coordinates = shell.Player?.AttachedEntity is { } adminEntity && _entityManager.TryGetComponent<TransformComponent>(adminEntity, out var adminTransform)
            ? adminTransform.Coordinates
            : _gameTicker.GetObserverSpawnPoint();

        var character = (HumanoidCharacterProfile)_prefs.GetPreferences(player.UserId).SelectedCharacter;
        var mobUid = _spawningSystem.SpawnPlayerMob(coordinates, jobName, character, station: null);

        if (transferMind)
            _mindSystem.TransferTo(mindEnt, mobUid);

        if (doTraits)
        {
            var adminSpawn = new AdminSpawnCompleteEvent(mobUid, player, jobName, character);
            _entityManager.EventBus.RaiseLocalEvent(mobUid, adminSpawn, true);
        }

        shell.WriteLine("cmd-spawncharacter-complete");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(
                    CompletionHelper.SessionNames(true, _playerManager),
                    Loc.GetString("cmd-spawnplayer-arg-player")),
            2 => CompletionResult.FromHintOptions(
                    CompletionHelper.PrototypeIDs<JobPrototype>(),
                    Loc.GetString("cmd-spawncharacter-arg-job")),
            3 => CompletionResult.FromHintOptions(
                    CompletionHelper.Booleans,
                    Loc.GetString("cmd-spawnplayer-arg-traits")),
            4 => CompletionResult.FromHintOptions(
                    CompletionHelper.Booleans,
                    Loc.GetString("cmd-spawnplayer-arg-transfermind")),
            _ => CompletionResult.Empty
        };
    }
}
