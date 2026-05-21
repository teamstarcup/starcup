using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Weather;

[RegisterComponent, NetworkedComponent]
public sealed partial class WeatherDamageStatusEffectComponent : Component
{
    /// <summary>
    /// DeltaV: Damage you can take from being in this weather.
    /// Only applies when weather has fully set in.
    /// </summary>
    [DataField]
    public DamageSpecifier? Damage;

    /// <summary>
    /// DeltaV: Don't damage entities that match this blacklist.
    /// </summary>
    [DataField]
    public EntityWhitelist? DamageBlacklist;

    /// <summary>
    /// starcup: Status effect that's applied to entities exposed to this weather.
    /// </summary>
    [DataField]
    public EntProtoId? StatusEffect;

    /// <summary>
    /// starcup: Should the status effect refresh?
    /// </summary>
    [DataField]
    public bool Refresh = true;

    /// <summary>
    /// DeltaV: How long to wait between updating weather effects.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// DeltaV: When to next update weather effects (damage).
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
