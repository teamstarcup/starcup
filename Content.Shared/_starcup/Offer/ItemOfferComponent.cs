using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Hands;

/// <summary>
/// Allows an entity to offer, and receive offers for, items. Adapted from Starlight changes to HandsComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ItemOfferComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool Offering;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool ReceivingOffer;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? OfferItem;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? OfferTarget;
}
