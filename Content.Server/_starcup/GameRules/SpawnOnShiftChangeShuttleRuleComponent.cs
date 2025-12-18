using Robust.Shared.Utility;

namespace Content.Server._starcup.GameRules;

[RegisterComponent]
public sealed partial class SpawnOnShiftChangeShuttleRuleComponent : Component
{
    [DataField(required: true)]
    public ResPath ShuttleGrid;

    [DataField]
    public TimeSpan? TravelTime;

    [DataField]
    public bool StopAfterArrival = true;

    /// <summary>
    /// Set to true once the shuttle has arrived at the station. Skips spawning players on the shuttle if
    /// StopAfterArrival has been set to true.
    /// </summary>
    [DataField]
    public bool HasShuttleArrived;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid Shuttle;
}
