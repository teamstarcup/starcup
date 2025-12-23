using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._starcup.Resting;

/// <summary>
/// Component that allows an entity to heal when sleeping. This is separate from the healing given by beds
/// which will override this healing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class HealOnSleepComponent : Component
{
    /// <summary>
    /// Damage to apply to the entity when it sleeps
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// The time between healing ticks, in seconds.
    /// </summary>
    [DataField(required: false)]
    public float HealTime = 1f;

    // <summary>
    // The next time that healing should be applied.
    // </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan NextHealTime = TimeSpan.Zero;
}
