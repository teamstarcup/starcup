using Content.Shared._starcup.Chaplain; // starcup
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes; // starcup

namespace Content.Shared.Bible.Components;

/// <summary>
/// Marks entity as bible user.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(new[] { typeof(BibleSystem), typeof(SanctifyItemSystem) })] // starcup: allow sanctify to access
public sealed partial class BibleUserComponent : Component
{
    public override bool SendOnlyToOwner => true;

    /// <summary>
    /// starcup: If the bible user has sanctified an item, they may only be able to sanctify
    /// another item of the same prototype should the original ever be destroyed.
    /// </summary>
    [DataField]
    public EntProtoId? SanctifiedArchetype;
}
