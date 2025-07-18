using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._starcup.Spawners.EntitySystems;

/// <summary>
/// System for jobs that are always forced to spawn at job specific spawn points and never at general ones
/// </summary>
public sealed class ForcedJobSpawnPointSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        // Run before the base containers spawn system since it allows both job and non-job containers on late join
        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning, before: [typeof(ContainerSpawnPointSystem)]);
    }

    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        // Another system has already spawned the player
        if (args.SpawnResult != null)
            return;

        if (!_prototypeManager.TryIndex(args.Job, out var jobPrototype) || !jobPrototype.AlwaysUseJobSpawn)
            return;

        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();

        while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (spawnPoint.SpawnType != SpawnPointType.Job || spawnPoint.Job != args.Job)
                continue;

            if (args.Station != null && _stationSystem.GetOwningStation(uid, xform) != args.Station)
                continue;

            possiblePositions.Add(xform.Coordinates);
        }

        // We didn't find any positions, leave it up to default spawning to handle
        if (possiblePositions.Count == 0)
        {
            Log.Warning($"No job spawn point found on station for {args.Job}");
            return;
        }

        var spawnLoc = _random.Pick(possiblePositions);

        args.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            args.Job,
            args.HumanoidCharacterProfile,
            args.Station);
    }
}
