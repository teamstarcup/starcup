using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.Preferences;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._starcup.Spawners.EntitySystems;

public sealed class ForcedJobContainerSpawnPointSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawningEvent>(HandlePlayerSpawning, before: [typeof(ForcedJobSpawnPointSystem)]);
    }

    private void HandlePlayerSpawning(PlayerSpawningEvent args)
    {
        // Another system has already spawned the player
        if (args.SpawnResult != null)
            return;

        if (args.HumanoidCharacterProfile?.SpawnPriority != SpawnPriorityPreference.Cryosleep)
            return;

        if (!_prototypeManager.TryIndex(args.Job, out var jobPrototype) || !jobPrototype.AlwaysUseJobSpawn)
            return;

        var query = EntityQueryEnumerator<ContainerSpawnPointComponent, ContainerManagerComponent, TransformComponent>();
        var possibleContainers = new List<Entity<ContainerSpawnPointComponent, ContainerManagerComponent, TransformComponent>>();

        while (query.MoveNext(out var uid, out var spawnPoint, out var container, out var xform))
        {
            if (spawnPoint.SpawnType != SpawnPointType.Job || spawnPoint.Job != args.Job)
                continue;

            if (args.Station != null && _stationSystem.GetOwningStation(uid, xform) != args.Station)
                continue;

            possibleContainers.Add((uid, spawnPoint, container, xform));
        }

        if (possibleContainers.Count == 0)
            return;

        // Spawn the player entity at the first possible container.
        var baseCoords = possibleContainers[0].Comp3.Coordinates;

        args.SpawnResult = _stationSpawning.SpawnPlayerMob(
            baseCoords,
            args.Job,
            args.HumanoidCharacterProfile,
            args.Station);

        // Try to put the player entity into a container
        _random.Shuffle(possibleContainers);
        foreach (var (uid, spawnPoint, manager, xform) in possibleContainers)
        {
            if (!_container.TryGetContainer(uid, spawnPoint.ContainerId, out var container, manager))
                continue;

            if (!_container.Insert(args.SpawnResult.Value, container, containerXform: xform))
                continue;

            return;
        }

        // Couldn't place the player entity into any containers
        // Delete the entity and allow other spawn systems to handle
        Del(args.SpawnResult);
        args.SpawnResult = null;
    }
}
