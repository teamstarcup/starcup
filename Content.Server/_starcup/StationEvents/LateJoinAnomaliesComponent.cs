using Content.Shared.Access;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.StationEvents;

/// <summary>
///     Greytide Virus event specific configuration
/// </summary>
[RegisterComponent, Access(typeof(LateJoinAnomaliesRule))]
public sealed partial class LateJoinAnomaliesComponent : Component
{
    /// <summary>
    ///     Range from which the severity is randomly picked from.
    /// </summary>
    [DataField]
    public MinMax SeverityRange = new(1, 3);

    /// <summary>
    ///     Severity corresponding to the number of access groups affected.
    ///     Will pick randomly from the SeverityRange if not specified.
    /// </summary>
    [DataField]
    public int? Severity;
}
