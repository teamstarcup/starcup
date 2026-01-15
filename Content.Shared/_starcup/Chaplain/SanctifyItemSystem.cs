using Content.Shared.Actions;
using Content.Shared.Bible.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.IdentityManagement;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared._starcup.Chaplain;

public sealed class SanctifyItemSystem : EntitySystem
{
    [Dependency] private readonly ISerializationManager _seriMan = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly NameModifierSystem _nameMod = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public static readonly EntProtoId BaseSanctifiedItemProtoId = "BaseSanctifiedItem";
    public static readonly EntProtoId SanctifyActionProtoId = "ActionSanctifyItem";
    public static readonly EntProtoId HealActionProtoId = "ActionBlessedHealing";

    public override void Initialize()
    {
        SubscribeLocalEvent<SanctifyItemEvent>(OnSanctifyItem);
        SubscribeLocalEvent<SanctifiedComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<SanctifiedComponent, ComponentRemove>(OnDestroyed);
        SubscribeLocalEvent<BibleUserComponent, ComponentStartup>(OnBibleUserSpawn);
        SubscribeLocalEvent<SanctifiedComponent, RefreshNameModifiersEvent>(OnRefreshNameModifiers);
    }

    private void OnBibleUserSpawn(Entity<BibleUserComponent> entity, ref ComponentStartup args)
    {
        _actions.AddAction(entity.Owner, SanctifyActionProtoId);
    }

    private void OnSanctifyItem(SanctifyItemEvent ev)
    {
        if (ev.Handled || !TryComp<BibleUserComponent>(ev.Performer, out var bibleUser))
            return;

        ev.Handled = true;

        // should this item ever be destroyed, the player may only sanctify another item of the same prototype
        if (TryComp(ev.Target, out MetaDataComponent? metaData) && bibleUser.SanctifiedArchetype is not null)
        {
            if (bibleUser.SanctifiedArchetype != metaData.EntityPrototype?.ID)
            {
                var failMessage = Loc.GetString("sanctify-failure-wrong-archetype",
                    ("target", Identity.Entity(ev.Target, EntityManager)));

                _popup.PopupEntity(failMessage, ev.Performer, ev.Performer, PopupType.Large);

                return;
            }
        }

        if (!_protoMan.Resolve(BaseSanctifiedItemProtoId, out var prototype))
            return;

        // take each prototype provided to the base holy item and apply it to the sanctified item
        foreach (var (name, entry) in prototype.Components)
        {
            var reg = _componentFactory.GetRegistration(name);
            EntityPrototype.EnsureCompExistsAndDeserialize(ev.Target,
                reg,
                _componentFactory,
                EntityManager,
                _seriMan,
                name,
                entry.Component,
                null
                );
        }

        // apply the sanctified component to the item, and assign it an owner
        EnsureComp<SanctifiedComponent>(ev.Target, out var sanctified);
        sanctified.OwnerUid = ev.Performer;

        // if the item can already be used as a melee weapon, give it an additional holy damage value
        if (EnsureComp<MeleeWeaponComponent>(ev.Target, out var meleeWeapon))
        {
            meleeWeapon.Damage.DamageDict["Holy"] = 25f;
            Dirty(ev.Target, meleeWeapon);
        }
        else
        // if it can't, give it a damage dictionary with a single entry for holy damage
        {
            meleeWeapon.Damage = new DamageSpecifier
            {
                DamageDict =
                {
                    ["Holy"] = 25f,
                },
            };
        }

        // track the player's sanctified item archetype
        bibleUser.SanctifiedArchetype = metaData?.EntityPrototype?.ID;

        var successMessage = Loc.GetString("sanctify-success",
            ("target", Identity.Entity(ev.Target, EntityManager)));
        _popup.PopupEntity(successMessage, ev.Performer, ev.Performer, PopupType.Large);

        var sanctifySound = new SoundPathSpecifier("/Audio/_starcup/Magic/sanctify.ogg");
        _audio.PlayPvs(sanctifySound, ev.Performer);

        // prompts OnRefreshNameModifiers so the item can be given a prefix
        _nameMod.RefreshNameModifiers(ev.Target);

        // remove the sanctify action from the player
        _actions.RemoveAction(ev.Performer, ev.Action!);
    }

    private void OnGetActions(Entity<SanctifiedComponent> entity, ref GetItemActionsEvent args)
    {
        // if a non-chaplain acquires a sanctified item, the action won't appear for them
        if (!HasComp<BibleUserComponent>(args.User))
            return;

        args.AddAction(ref entity.Comp.HealingActionUid, HealActionProtoId);
    }

    private void OnRefreshNameModifiers(Entity<SanctifiedComponent> entity, ref RefreshNameModifiersEvent args)
    {
        args.AddModifier("sanctified-item-prefix");
    }

    private void OnDestroyed(Entity<SanctifiedComponent> entity, ref ComponentRemove args)
    {
        if (entity.Comp.OwnerUid is null)
            return;

        var destroyedMessage = Loc.GetString("sanctified-item-destroyed",
            ("target", Identity.Entity(entity, EntityManager)));
        _popup.PopupEntity(destroyedMessage, entity.Comp.OwnerUid.Value, entity.Comp.OwnerUid.Value, PopupType.Large);

        var destroyedSound = new SoundPathSpecifier("/Audio/_starcup/Magic/sanctify_lost.ogg");
        _audio.PlayPvs(destroyedSound, entity.Comp.OwnerUid.Value);

        // give them the sanctify action back
        _actions.AddAction(entity.Comp.OwnerUid.Value, SanctifyActionProtoId);
    }
}
