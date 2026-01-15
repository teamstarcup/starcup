using System.Linq;
using Content.Server.Administration;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SpawnCharacter : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly StationSpawningSystem _spawningSystem = default!;

    public override string Command => "spawncharacter";
    public override string Description => Loc.GetString("cmd-spawncharacter-desc");
    public override string Help => Loc.GetString("cmd-spawncharacter-help");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        // Requirements
        // Must be run from a client
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (player.AttachedEntity is not { } attachedEntity)
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

        // Argument handling;
        // Argument 1: Character, Optional
        HumanoidCharacterProfile character;
        if (args.Length < 1)
        {
            character = (HumanoidCharacterProfile)_prefs.GetPreferences(player.UserId).SelectedCharacter;
        }
        else
        {
            var name = args[0]; // Auto-complete adds quotes around the name, so no need to worry about spaces.
            if (!TryFetchCharacter(player.UserId, name, out character))
            {
                shell.WriteError(Loc.GetString("cmd-spawncharacter-error-character", ("name", name)));
                return;
            }
        }

        // Argument 2: Job, Optional
        var jobName = args.Length > 1 ? args[1] : "Passenger"; // just default to passenger

        if (!_prototypeManager.HasIndex<JobPrototype>(jobName))
        {
            shell.WriteError(Loc.GetString("cmd-spawncharacter-error-job"));
            return;
        }

        // Argument 3: apply traits, Optional
        var doTraits = false;
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

        // Perform the command
        var coordinates = _entityManager.GetComponent<TransformComponent>(attachedEntity).Coordinates;

        var mobUid = _spawningSystem.SpawnPlayerMob(coordinates, jobName, character, station: null);

        if (transferMind)
            _mindSystem.TransferTo(mindEnt, mobUid);

        if (doTraits)
        {
            var adminSpawn = new AdminSpawnCompleteEvent(mobUid, player, jobName, character);
            _entityManager.EventBus.RaiseLocalEvent(mobUid, adminSpawn, true);
        }

        shell.WriteLine(Loc.GetString("cmd-spawncharacter-complete"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1: // Character name
                if (shell.Player is not { } player)
                    return CompletionResult.Empty;

                if (player.GetMind() == null)
                    return CompletionResult.Empty;

                if (!FetchCharacters(player.UserId, out var characters))
                    return CompletionResult.Empty;

                return CompletionResult.FromHintOptions(
                        characters.Select(c => c.Name),
                        Loc.GetString("cmd-spawncharacter-arg-character"));
            case 2: // Job prototype
                return CompletionResult.FromHintOptions(
                        CompletionHelper.PrototypeIDs<JobPrototype>(),
                        Loc.GetString("cmd-spawncharacter-arg-job"));
            case 3: // Whether or not traits should be applied
                return CompletionResult.FromHintOptions(
                        CompletionHelper.Booleans,
                        Loc.GetString("cmd-spawncharacter-arg-traits"));
            case 4: // Whether or not to transfer the player's mind to the new mob
                return CompletionResult.FromHintOptions(
                        CompletionHelper.Booleans,
                        Loc.GetString("cmd-spawncharacter-arg-transfermind"));
            default:
                return CompletionResult.Empty;
        }
    }

    private bool TryFetchCharacter(NetUserId player, string name, out HumanoidCharacterProfile character)
    {
        character = null!;

        if (!FetchCharacters(player, out var characters))
            return false;

        var selected = characters.FirstOrDefault(c => c.Name == name);

        if (selected == null)
            return false;

        character = selected;
        return true;
    }

    private bool FetchCharacters(NetUserId player, out List<HumanoidCharacterProfile> characters)
    {
        characters = [];
        if (!_prefs.TryGetCachedPreferences(player, out var prefs))
            return false;

        foreach (var (_, character) in prefs.Characters)
        {
            if (character is HumanoidCharacterProfile humanoid)
                characters.Add(humanoid);
        }

        return true;
    }
}

// Largely a copy of PlayerSpawnComplete - used to apply traits without running the other systems
public sealed class AdminSpawnCompleteEvent(
        EntityUid mob, ICommonSession player, string? jobId, HumanoidCharacterProfile profile
) : EntityEventArgs
{
    public EntityUid Mob { get; } = mob;
    public ICommonSession Player { get; } = player;
    public string? JobId { get; } = jobId;
    public HumanoidCharacterProfile Profile { get; } = profile;
}
