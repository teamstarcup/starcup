using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.Spawners.EntitySystems;

/// <summary>
/// Used for manipulating what spawn points are viable for late-joins. This is used primarily for determining if a late-join
/// or cryo bed spawn point is suitable for spawning a specific job (e.g. prisoners.) <see cref="SpawnPointSystem"/>,
/// <see cref="ContainerSpawnPointSystem"/> still check for the usual conditions.
/// </summary>
public sealed class RestrictiveSpawnPointSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CheckSpawnPointSuitabilityEvent<SpawnPointComponent>>(HandleCheckSpawnPointSuitability);
        SubscribeLocalEvent<CheckSpawnPointSuitabilityEvent<ContainerSpawnPointComponent>>(HandleCheckSpawnPointSuitabilityCryo);
    }

    private void HandleCheckSpawnPointSuitability(ref CheckSpawnPointSuitabilityEvent<SpawnPointComponent> args)
    {
        if (!_prototypeManager.TryIndex(args.Job, out var jobPrototype) || !jobPrototype.AlwaysUseJobSpawn)
        {
            return;
        }

        var spawnPoint = args.SpawnPointEntity.Comp;
        if (spawnPoint.SpawnType != SpawnPointType.Job || spawnPoint.Job != args.Job)
        {
            args.NotSuitable = true;
        }
    }

    private void HandleCheckSpawnPointSuitabilityCryo(ref CheckSpawnPointSuitabilityEvent<ContainerSpawnPointComponent> args)
    {
        if (!_prototypeManager.TryIndex(args.Job, out var jobPrototype) || !jobPrototype.AlwaysUseJobSpawn)
        {
            return;
        }

        var spawnPoint = args.SpawnPointEntity.Comp;
        if (spawnPoint.SpawnType != SpawnPointType.Job || spawnPoint.Job != args.Job)
        {
            args.NotSuitable = true;
        }

        if (args.HumanoidCharacterProfile?.SpawnPriority != SpawnPriorityPreference.Cryosleep)
            args.NotSuitable = true;
    }
}
