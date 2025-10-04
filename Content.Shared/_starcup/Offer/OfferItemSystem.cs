using Content.Shared.Alert;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Hands;

/// <summary>
/// Facilitates item offering interactions between players. Adapted from Starlight's SharedHandsSystem.Offer.cs
/// </summary>
public sealed class OfferItemSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHands = default!;
    [Dependency] private readonly SharedTransformSystem _sharedTransform = default!;

    private readonly ProtoId<AlertPrototype> _offerAlert = "Offer";

    public override void Initialize()
    {
        SubscribeLocalEvent<ItemOfferComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<ItemComponent, GotUnequippedHandEvent>(OnUnequipHand);
        SubscribeLocalEvent<ItemOfferComponent, OfferItemAlertEvent>(OnOfferItemAlertEvent);
        SubscribeLocalEvent<ItemOfferComponent, GetVerbsEvent<Verb>>(OfferItemVerb);
        SubscribeLocalEvent<ItemOfferComponent, DropHandItemsEvent>(OnDropHandItems);

        // Wow! This is bullshit but im not making a script to ServerSide.
        if (_net.IsServer)
            SubscribeLocalEvent<ItemComponent, DroppedEvent>(OnDropped);
    }

    private void OnOfferItemAlertEvent(Entity<ItemOfferComponent> ent, ref OfferItemAlertEvent args)
    {
        if (args.Handled || _net.IsClient)
            return;

        AcceptOffer((ent.Owner, ent.Comp));

        args.Handled = true;
    }

    private void AcceptOffer(Entity<ItemOfferComponent?> entity)
    {
        if (!Resolve(entity.Owner, ref entity.Comp, false))
            return;

        if (TryComp<ItemOfferComponent>(entity.Comp.OfferTarget, out var targetOffer))
            targetOffer.ReceivingOffer = true;

        if (entity.Comp.OfferItem is not null)
        {
            if (!_sharedHands.TryPickupAnyHand(entity.Owner, entity.Comp.OfferItem.Value))
            {
                _popupSystem.PopupEntity(Loc.GetString("hands-full"), entity.Owner, entity.Owner);
                return;
            }

            if (entity.Comp.OfferTarget is not null)
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("offered-target",
                        ("item", entity.Comp.OfferItem),
                        ("user", entity.Comp.OfferTarget.Value)),
                    entity.Comp.OfferTarget.Value,
                    entity.Owner);
                _popupSystem.PopupEntity(Loc.GetString("offered", ("item", entity.Comp.OfferItem)),
                    entity.Comp.OfferTarget.Value,
                    entity.Comp.OfferTarget.Value);
                EndOffer((entity.Comp.OfferTarget.Value, targetOffer ?? null), false);
            }
        }

        EndOffer((entity.Owner, entity.Comp), false);
    }

    private void OfferItemVerb(EntityUid uid, ItemOfferComponent offerComp, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.User == args.Target
            || args.Using is null || HasComp<UnremoveableComponent>(args.Using) || HasComp<VirtualItemComponent>(args.Using)
            || offerComp.Offering || offerComp.ReceivingOffer
            || !TryComp<ItemOfferComponent>(args.User, out var targetOffer)
            || targetOffer.Offering || targetOffer.ReceivingOffer)
            return;

        args.Verbs.Add(new Verb
        {
            Act = () => OfferItem(args.User, uid, targetOffer, offerComp),
            DoContactInteraction = true,
            Text = Loc.GetString("offer", ("item", args.Using)),
            IconEntity = GetNetEntity(args.Using)
        });
    }

    /// <summary>
    /// Offer the ActiveItem to the target (if they have an ItemOfferComponent)
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="target"></param>
    /// <param name="offerComp"></param>
    /// <param name="targetOffer"></param>
    private void OfferItem(EntityUid uid, EntityUid target, ItemOfferComponent? offerComp = null, ItemOfferComponent? targetOffer = null)
    {
        if (!Resolve(uid, ref offerComp, false)
            || !Resolve(target, ref targetOffer, false)
            || !_sharedHands.TryGetActiveItem(uid, out var heldItem))
            return;

        offerComp.OfferItem = heldItem;
        targetOffer.OfferItem = heldItem;

        offerComp.Offering = true;
        offerComp.OfferTarget = target;

        targetOffer.ReceivingOffer = true;
        targetOffer.OfferTarget = uid;

        Dirty(uid, offerComp);
        Dirty(target, targetOffer);
        _alertsSystem.ShowAlert(target, _offerAlert);

        if (!_net.IsServer)
            return;

        _popupSystem.PopupEntity(Loc.GetString("offering", ("item", heldItem), ("target", target)), uid, uid);
        _popupSystem.PopupEntity(Loc.GetString("offering-target", ("item", heldItem), ("user", uid)), uid, target);
    }

    /// <summary>
    /// End the Offer, this can be used to cancel or end the process.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="popups"></param>
    private void EndOffer(Entity<ItemOfferComponent?> entity, bool? popups = null)
    {
        if (!Resolve(entity.Owner, ref entity.Comp))
            return;

        if (popups ?? true)
        {
            if (entity.Comp.OfferItem is not null)
            {
                if (!entity.Comp.ReceivingOffer)
                {
                    _popupSystem.PopupEntity(Loc.GetString("unoffer",
                            ("item", entity.Comp.OfferItem)),
                        entity.Owner,
                        entity.Owner);
                }
                else if (entity.Comp.OfferTarget is not null)
                {
                    _popupSystem.PopupEntity(Loc.GetString("unoffer-target",
                            ("item", entity.Comp.OfferItem),
                            ("user", entity.Comp.OfferTarget)),
                        entity.Comp.OfferTarget.Value,
                        entity.Owner);
                }
            }
        }

        _alertsSystem.ClearAlert(entity.Owner, _offerAlert);

        entity.Comp.Offering = false;
        entity.Comp.ReceivingOffer = false;
        entity.Comp.OfferItem = null;
        entity.Comp.OfferTarget = null;

        Dirty(entity);
    }

    private void OnMove(EntityUid uid, ItemOfferComponent comp, MoveEvent args)
    {
        if (comp.OfferTarget is null)
            return;

        var inRange = _sharedTransform.InRange(args.NewPosition, Transform(comp.OfferTarget.Value).Coordinates, 2f);
        if (inRange)
            return;

        EndOffer(comp.OfferTarget.Value);
        EndOffer((uid, comp));
    }

    private void OnDropped(EntityUid uid, ItemComponent item, DroppedEvent args)
    {
        if (!TryComp<ItemOfferComponent>(args.User, out var offerComp) || !offerComp.Offering || offerComp.OfferItem != uid)
            return;

        if (offerComp.OfferTarget is not null)
            EndOffer(offerComp.OfferTarget.Value);

        EndOffer((args.User, offerComp));
    }

    private void OnUnequipHand(EntityUid uid, ItemComponent ignored, GotUnequippedHandEvent args)
    {
        if (_net.IsClient || !TryComp<ItemOfferComponent>(args.User, out var offerComp) || !offerComp.Offering || offerComp.OfferItem != uid)
            return;

        // This check is to make sure OnUnequipHand when AcceptOffer, Silly but works!
        if (offerComp is { Offering: true, ReceivingOffer: true })
            return;

        if (offerComp.OfferTarget is not null)
            EndOffer(offerComp.OfferTarget.Value);

        EndOffer((args.User, offerComp));
    }

    /// <summary>
    /// End offers when the offering player falls (or otherwise is forced to drop their held items).
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="args"></param>
    private void OnDropHandItems(Entity<ItemOfferComponent> entity, ref DropHandItemsEvent args)
    {
        if (entity.Comp is { Offering: false })
            return;

        if (entity.Comp.OfferTarget is not null)
            EndOffer(entity.Comp.OfferTarget.Value);

        EndOffer((entity.Owner, entity.Comp));
    }
}

public sealed partial class OfferItemAlertEvent : BaseAlertEvent; // Starlight
