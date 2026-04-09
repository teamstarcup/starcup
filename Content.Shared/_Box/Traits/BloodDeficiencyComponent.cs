using Robust.Shared.GameStates;
using Content.Shared.Body.Systems;

namespace Content.Shared._Box.Traits.Assorted;

/// <summary>
/// Used for the blood deficiency trait. BloodstreamSystem will check for this component and modify blood regen amount accordingly.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBloodstreamSystem))]
public sealed partial class BloodDeficiencyComponent : Component
{
    [DataField("bloodRegenAmount"), ViewVariables(VVAccess.ReadWrite)]
    public float BloodRegenAmount = 1f;
    [DataField("bloodLevelTarget"), ViewVariables(VVAccess.ReadWrite)]
    public float BloodLevelTarget = 1f;
}
