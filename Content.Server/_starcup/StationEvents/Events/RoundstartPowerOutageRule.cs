using Content.Server._starcup.StationEvents.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.VariationPass;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Power.Components;

namespace Content.Server._starcup.StationEvents.Events;

/// <summary>
/// Drains the power from every APC, Substation, and SMES from the station at round start.
/// </summary>
public sealed class RoundstartPowerOutageRule : VariationPassSystem<RoundstartPowerOutageComponent>
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    protected override void ApplyVariation(Entity<RoundstartPowerOutageComponent> ent, ref StationVariationPassEvent args)
    {
        var query = AllEntityQuery<BatteryComponent, PowerNetworkBatteryComponent>();
        while (query.MoveNext(out var entity, out var battery, out _))
        {
            if (_station.GetOwningStation(entity) != args.Station)
                continue;

            _battery.SetCharge((entity, battery), 0);
        }
    }
}
