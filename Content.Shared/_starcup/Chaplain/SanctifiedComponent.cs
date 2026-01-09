using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Shared._starcup.Chaplain;

[RegisterComponent]
public sealed partial class SanctifiedComponent : Component
{
    /// <summary>
    /// The owner of this item.
    /// </summary>
    [DataField]
    public EntityUid? OwnerUid;

    /// <summary>
    /// The prototype of the Blessed Healing action granted by this item.
    /// </summary>
    [DataField]
    public EntityUid? HealingActionUid;

    /// <summary>
    /// Damage that will be healed upon successful use of the Blessed Healing action.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// The sound that plays upon successful use of the Blessed Healing action.
    /// </summary>
    [DataField]
    public SoundSpecifier HealSound = new SoundPathSpecifier("/Audio/Magic/staff_healing.ogg");
}
