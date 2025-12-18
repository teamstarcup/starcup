using Content.Server._starcup.Shuttles;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;

namespace Content.Server._starcup.GameRules;

public sealed class ReturnShiftChangeShuttleRuleSystem : GameRuleSystem<ReturnShiftChangeShuttleRuleComponent>
{
    [Dependency] private readonly ShiftChangeShuttleSystem _shiftChangeShuttle = default!;

    protected override void Started(EntityUid uid, ReturnShiftChangeShuttleRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        if (!_shiftChangeShuttle.TryGetShiftChangeShuttle(out var shiftChangeShuttle))
        {
            Log.Warning("Unable to find shift change shuttle");
            return;
        }

        _shiftChangeShuttle.QueueStop(shiftChangeShuttle.Value,
            new ShiftChangeShuttleStop
            {
                Destination = shiftChangeShuttle.Value.Comp.DepotGrid,
                PreDepartureTime = component.After,
                DumpPassengersBeforeDeparture = true,
            });
    }
}
