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
    /// <summary>
    /// The maximum amount of blood, in units, we are allowed to add or remove per update. This should generally be positive; negative numbers will do nothing.
    /// In the case of the blood deficiency trait, this is set to 0.08f, as this is the amount of blood we want to modify the moob's blood volume by per update.
    /// In the case of the minor blood deficiency trait, this is set to 0f, completely halting natural blood regeneration.
    /// </summary>
    [DataField("bloodRefreshAmount"), ViewVariables(VVAccess.ReadWrite)]
    public float BloodRefreshAmount = 1f;

    /// <summary>
    /// The volume of blood that natural regeneration will aim for, expressed as a multiplier of the mob's maximum blood volume.
    /// For most humanoid mobs, the maximum volume is 300u; A value of 0.5 means that every 3 seconds, natural blood regeneration will attempt to add or remove up to {BloodRefreshAmount}u of reagent until the mob's bloodstream reaches 150u.
    /// In the case of the blood defiiciency trait, this is set to 0f, so that natural blood regeneration will always tick downwards towards 0u.
    /// </summary>
    [DataField("bloodReferenceFactor"), ViewVariables(VVAccess.ReadWrite)]
    public float BloodReferenceFactor = 1f;
}
