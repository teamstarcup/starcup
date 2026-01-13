using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server.Chat.Managers;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Screens.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.Components;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Collections;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._starcup.Shuttles;

public sealed class ShiftChangeShuttleSystem : EntitySystem
{

    private static readonly Vector2 ShuttleSpawnOffset = new(9000, 9000);

    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ActorSystem _actor = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShiftChangeShuttleComponent, FTLStartedEvent>(OnFTLStarted);
        SubscribeLocalEvent<ShiftChangeShuttleComponent, FTLCompletedEvent>(OnFTLCompleted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShiftChangeShuttleComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var shiftChangeShuttle))
        {
            if (curTime < shiftChangeShuttle.NextStopTime)
                continue;

            // if (shiftChangeShuttle.CurrentStop is not null)
            //     shiftChangeShuttle.PreDeparture = !shiftChangeShuttle.PreDeparture;
            //
            // if (shiftChangeShuttle is { PreDeparture: true, CurrentStop: not null })
            // {
            //     shiftChangeShuttle.NextStopTime = shiftChangeShuttle.CurrentStop.PreDepartureTime;
            //     continue;
            // }

            if (!shiftChangeShuttle.TravelQueue.TryDequeue(out var currentStop))
                continue;

            shiftChangeShuttle.CurrentStop = currentStop;

            shiftChangeShuttle.NextStopTime = curTime;
            shiftChangeShuttle.NextStopTime += currentStop.StartupTime;
            shiftChangeShuttle.NextStopTime += currentStop.TravelTime;
            shiftChangeShuttle.NextStopTime += currentStop.IdleTime;
            if (shiftChangeShuttle.TravelQueue.TryPeek(out var nextStop))
                shiftChangeShuttle.NextStopTime += nextStop.PreDepartureTime;

            ShuttleComponent? shuttleComp = null;
            if (!Resolve(uid, ref shuttleComp))
                continue;

            if (currentStop.DumpPassengersBeforeDeparture)
            {
                // try to dump passengers onto a docked grid with late join spawn points
                var dockedGrids = GetDockedGrids(uid);
                var pendingQuery = AllEntityQuery<MobStateComponent, TransformComponent>();
                while (pendingQuery.MoveNext(out var mobUid, out _, out var xform))
                {
                    if (xform.GridUid != uid)
                        continue;

                    var dockedGridsEnumerator = dockedGrids.GetEnumerator();
                    do
                    {
                        if (!TryTeleportToSpawnPoint(mobUid, dockedGridsEnumerator.Current, xform))
                            continue;

                        RemCompDeferred<AutoOrientComponent>(mobUid);
                        break;
                    } while (dockedGridsEnumerator.MoveNext());

                    // we didn't find anywhere to dump this chump!!!
                    Log.Warning($"Unable to dump passenger {mobUid} from shift change shuttle!");
                }
            }

            _shuttle.FTLToDock(uid,
                shuttleComp,
                currentStop.Destination,
                (float?)currentStop.StartupTime.TotalSeconds,
                (float?)currentStop.TravelTime.TotalSeconds,
                currentStop.DockingPriorityTag);
        }
    }

    private List<EntityUid> GetDockedGrids(EntityUid grid)
    {
        return EntityQuery<DockingComponent, TransformComponent>()
            .Where(comps => comps.Item2.GridUid == grid)
            .Where(comps => comps.Item1.Docked)
            .Select(comps => _transform.GetGrid(comps.Item1.DockedWith!.Value))
            .Where(uid => uid.HasValue && uid.Value != EntityUid.Invalid)
            .Select(uid => uid!.Value)
            .ToList();
    }

    /// <summary>
    /// Attempts to teleport the given player to a random late join spawn point on the given grid.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="gridUid"></param>
    /// <param name="transform"></param>
    /// <returns>true if a suitable spawn point was found</returns>
    private bool TryTeleportToSpawnPoint(EntityUid player, EntityUid gridUid, TransformComponent? transform = null)
    {
        if (!Resolve(player, ref transform))
            return false;

        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new ValueList<EntityCoordinates>(32);

        while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (spawnPoint.SpawnType == SpawnPointType.LateJoin && _transform.ContainsEntity(gridUid, uid))
            {
                possiblePositions.Add(xform.Coordinates);
            }
        }

        if (possiblePositions.Count <= 0)
            return false;

        _transform.SetCoordinates(player, transform, _random.Pick(possiblePositions));
        if (_actor.TryGetSession(player, out var session))
        {
            _chatManager.DispatchServerMessage(session!, Loc.GetString("latejoin-arrivals-teleport-to-spawn"));
        }
        return true;

    }

    private TimeSpan CalculateDepartureTime(Entity<ShiftChangeShuttleComponent> shuttle)
    {
        if (shuttle.Comp.TravelQueue.Count <= 0)
            return TimeSpan.Zero;

        var time = shuttle.Comp.NextStopTime;
        return time;
    }

    private void BroadcastShuttleDepartureTime(Entity<ShiftChangeShuttleComponent> shuttle, EntityUid mapUid)
    {
        if (!TryComp<DeviceNetworkComponent>(shuttle.Owner, out var netComp))
            return;

        object departureTime = CalculateDepartureTime(shuttle) - _timing.CurTime;
        var payload = new NetworkPayload
        {
            [ShuttleTimerMasks.ShuttleMap] = shuttle.Owner,
            [ShuttleTimerMasks.ShuttleTime] = departureTime,
            [ShuttleTimerMasks.SourceMap] = mapUid,
            [ShuttleTimerMasks.SourceTime] = departureTime,
            [ShuttleTimerMasks.Docked] = true,
        };
        _deviceNetworkSystem.QueuePacket(shuttle.Owner, null, payload, netComp.TransmitFrequency);
    }

    private void OnFTLStarted(Entity<ShiftChangeShuttleComponent> shuttle, ref FTLStartedEvent args)
    {
        if (!TryComp<DeviceNetworkComponent>(shuttle.Owner, out var netComp))
            return;

        TryComp<FTLComponent>(shuttle.Owner, out var ftlComp);
        var ftlTime = TimeSpan.FromSeconds(ftlComp?.TravelTime ?? _shuttle.DefaultTravelTime);

        var payload = new NetworkPayload
        {
            [ShuttleTimerMasks.ShuttleMap] = shuttle.Owner,
            [ShuttleTimerMasks.ShuttleTime] = ftlTime
        };

        payload.Add(ShuttleTimerMasks.DestMap, Transform(args.TargetCoordinates.EntityId).MapUid);
        payload.Add(ShuttleTimerMasks.DestTime, ftlTime);

        payload.Add(ShuttleTimerMasks.SourceMap, args.FromMapUid);
        payload.Add(ShuttleTimerMasks.SourceTime, ftlTime);

        _deviceNetworkSystem.QueuePacket(shuttle.Owner, null, payload, netComp.TransmitFrequency);
    }

    private void OnFTLCompleted(Entity<ShiftChangeShuttleComponent> shuttle, ref FTLCompletedEvent args)
    {
        BroadcastShuttleDepartureTime(shuttle, args.MapUid);
    }

    public bool SpawnShiftChangeShuttle(MapId mapId, ResPath shuttleGridPath, EntityUid depotGrid, [NotNullWhen(true)] out Entity<ShiftChangeShuttleComponent>? shiftChangeShuttle)
    {
        if (!_loader.TryLoadGrid(mapId, shuttleGridPath, out var shuttle, offset: ShuttleSpawnOffset))
        {
            shiftChangeShuttle = null;
            return false;
        }

        var shiftChangeShuttleComponent = EnsureComp<ShiftChangeShuttleComponent>(shuttle.Value.Owner);
        shiftChangeShuttleComponent.DepotGrid = depotGrid;
        shiftChangeShuttle = (shuttle.Value.Owner, shiftChangeShuttleComponent);
        return true;
    }

    public void QueueStop(Entity<ShiftChangeShuttleComponent> shuttle, ShiftChangeShuttleStop stop)
    {
        // TODO: Edge case; queueing a stop after a shuttle is docked does not yet update the shuttle timers
        // TODO: Edge case; NextStopTime sometimes needs to be recalculated according to PreDepartureTime

        shuttle.Comp.TravelQueue.Enqueue(stop);
    }

    public bool TryGetShiftChangeShuttle([NotNullWhen(true)] out Entity<ShiftChangeShuttleComponent>? shuttle)
    {
        var arrivalsQuery = EntityQueryEnumerator<ShiftChangeShuttleComponent>();
        while (arrivalsQuery.MoveNext(out var uid, out var comp))
        {
            shuttle = (uid, comp);
            return true;
        }

        shuttle = null;
        return false;
    }
}
