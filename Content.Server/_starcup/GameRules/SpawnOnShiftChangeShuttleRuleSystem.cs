using System.Numerics;
using Content.Server._starcup.Shuttles;
using Content.Server.Buckle.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._starcup.CCVars;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._starcup.GameRules;

/// <summary>
/// Spawns players on a shift change shuttle.
/// </summary>
public sealed class SpawnOnShiftChangeShuttleRuleSystem : GameRuleSystem<SpawnOnShiftChangeShuttleRuleComponent>
{
    [Dependency] private readonly ShiftChangeShuttleSystem _shiftChangeShuttle = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly BuckleSystem _buckle = default!;

    private EntityQuery<StrapComponent> _strapQuery;

    public override void Initialize()
    {
        base.Initialize();

        _strapQuery = GetEntityQuery<StrapComponent>();

        SubscribeLocalEvent<StationPostInitEvent>(OnStationPostInit);

        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning,
            before: [typeof(SpawnPointSystem), typeof(ContainerSpawnPointSystem)]);
        SubscribeLocalEvent<FTLCompletedEvent>(OnFTLCompleted);
    }

    private void OnStationPostInit(ref StationPostInitEvent args)
    {
        Entity<SpawnOnShiftChangeShuttleRuleComponent>? rule = null;
        var ruleQuery = EntityQueryEnumerator<SpawnOnShiftChangeShuttleRuleComponent>();
        while (ruleQuery.MoveNext(out var ruleUid, out var ruleComp))
        {
            rule = (ruleUid, ruleComp);
            break;
        }

        if (rule is null)
            return;

        StationCentcommComponent? stationCentcommComponent = null;
        if (!Resolve(args.Station, ref stationCentcommComponent))
            return;

        if (!stationCentcommComponent.MapEntity.HasValue || !stationCentcommComponent.Entity.HasValue)
        {
            Log.Error("Could not find Centcomm!");
            return;
        }

        var centcommMapId = _transform.GetMapId((stationCentcommComponent.MapEntity.Value, null));
        if (!_shiftChangeShuttle.SpawnShiftChangeShuttle(centcommMapId,
                rule.Value.Comp.ShuttleGrid,
                stationCentcommComponent.Entity.Value,
                out var shiftChangeShuttle))
        {
            Log.Error("Failed to spawn shift change shuttle.");
            return;
        }

        rule.Value.Comp.Shuttle = shiftChangeShuttle.Value.Owner;

        var destinationUid = _station.GetLargestGrid(args.Station.Owner);
        if (destinationUid is null)
        {
            Log.Error("Failed to find main station's largest grid for destination");
            return;
        }

        _shiftChangeShuttle.QueueStop(shiftChangeShuttle.Value,
            new ShiftChangeShuttleStop
            {
                Destination = destinationUid.Value,
                StartupTime = TimeSpan.Zero,
                TravelTime = rule.Value.Comp.TravelTime ??
                             TimeSpan.FromSeconds(_configurationManager.GetCVar(SCCVars.ShiftChangeShuttleTravelTime)),
            });
    }

    private void OnPlayerSpawning(PlayerSpawningEvent ev)
    {
        if (ev.SpawnResult != null)
            return;

        var ruleComponents = EntityQueryEnumerator<SpawnOnShiftChangeShuttleRuleComponent>();
        if (!ruleComponents.MoveNext(out var ruleUid, out var ruleComp))
            return;

        if (ruleComp is { StopAfterArrival: true, HasShuttleArrived: true })
            return;

        var shuttleUid = ruleComp.Shuttle;
        ShiftChangeShuttleComponent? shiftChangeShuttle = null;
        if (!Resolve(shuttleUid, ref shiftChangeShuttle))
            return;

        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();
        while (points.MoveNext(out var entity, out var spawnPoint, out var xform))
        {
            if (spawnPoint.Job != null && ev.Job != spawnPoint.Job)
                continue;

            // if (spawnPoint.SpawnType == SpawnPointType.LateJoin && !ev.)
            //     continue;

            if (!_transform.ContainsEntity(shuttleUid, entity))
                continue;

            possiblePositions.Add(xform.Coordinates);
        }

        if (possiblePositions.Count <= 0)
            return;

        var spawnLoc = _random.Pick(possiblePositions);
        ev.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            ev.Job,
            ev.HumanoidCharacterProfile,
            ev.Station);

        EnsureComp<AutoOrientComponent>(ev.SpawnResult.Value);

        // buckle players if they're on a seat so they don't get knocked down by FTL
        var seats = _entityLookup.GetEntitiesIntersecting(spawnLoc);
        foreach (var seat in seats)
        {
            if (!_strapQuery.TryComp(seat, out var strapComponent))
                continue;

            if (strapComponent.BuckledEntities.Count > 0)
                continue;

            _buckle.TryBuckle(ev.SpawnResult.Value, null, seat, popup: false);
        }
    }

    private void OnFTLCompleted(ref FTLCompletedEvent args)
    {
        var ruleComponents = EntityQueryEnumerator<SpawnOnShiftChangeShuttleRuleComponent>();
        while (ruleComponents.MoveNext(out var ruleComponent))
        {
            if (ruleComponent.HasShuttleArrived)
                continue;

            if (ruleComponent.Shuttle != args.Entity)
                continue;

            ruleComponent.HasShuttleArrived = true;
        }
    }
}
