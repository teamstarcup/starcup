using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.StationEvents.Components;

[RegisterComponent]
public sealed partial class LateJoinAnomalySpawnComponent : Component
{
    [DataField]
    public EntityTableSelector AnomalyTable = new NestedSelector
    {
        TableId = "RandomAnomalyTable",
    };

    /// <summary>
    /// The number of anomalies to spawn when the game rule is run.
    /// </summary>
    [DataField]
    public int Amount = 3;

    /// <summary>
    /// The chance that a spawned anomaly will have criticality immediately induced.
    /// </summary>
    [DataField]
    public float CritChance = 0.25f;

    /// <summary>
    /// Anomaly stability is increased by this amount before inducing criticality.
    /// </summary>
    [DataField]
    public float StabilityIncrease = 0.25f;
}
