using Content.Server._starcup.StationEvents.Components;
using Content.Server.Anomaly;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.VariationPass;
using Content.Server.Station.Systems;
using Content.Shared.EntityTable;
using Content.Shared.Station.Components;
using Robust.Shared.Random;

namespace Content.Server._starcup.StationEvents.Events;

/// <summary>
/// Simulates anomalies having been spawned over an hour or so with nobody around to deal with them.
///
/// Spawns several anomalies around the station, pulses them a few times, and sometimes makes them go critical.
/// </summary>
public sealed class LateJoinAnomalySpawnRule : VariationPassSystem<LateJoinAnomalySpawnComponent>
{
    [Dependency] private readonly AnomalySystem _anomaly = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;

    protected override void ApplyVariation(Entity<LateJoinAnomalySpawnComponent> entity, ref StationVariationPassEvent args)
    {
        var stationGrid = _station.GetLargestGrid(new Entity<StationDataComponent?>(args.Station.Owner, args.Station.Comp));

        if (!stationGrid.HasValue)
        {
            Log.Error($"Failed to get grid for station {args.Station}");
            return;
        }

        foreach (var entityPrototype in _entityTable.GetSpawns(entity.Comp.AnomalyTable))
        {
            var anomaly = _anomaly.SpawnOnRandomGridLocation(stationGrid.Value, entityPrototype);
            if (!anomaly.HasValue)
            {
                Log.Warning("Failed to generate anomaly");
                continue;
            }

            if (_random.Prob(entity.Comp.CritChance))
            {
                _anomaly.StartSupercriticalEvent(anomaly.Value);
            }
        }
    }
}
