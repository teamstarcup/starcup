using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.MKC;

/// <summary>
/// starcup: Attached to a power core MKC organ to handle electricity-based hunger analogue for robots.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PowerCoreComponent : Component
{
    /// <summary>
    /// Time between each 'drink' from a battery
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1.75);

    /// <summary>
    /// How many joules the 'drinker' drains from a power source per do-after
    /// </summary>
    [DataField]
    public float JoulesPerDrain = 90f;

    /// <summary>
    /// Multiply entity's movement speed by this amount when the entity is on low power.
    /// </summary>
    [DataField]
    public float LowPowerMovementSpeedMultiplier = 0.75f;
}
