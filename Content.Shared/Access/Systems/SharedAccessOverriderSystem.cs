using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Access.Systems
{
    [UsedImplicitly]
    public abstract partial class SharedAccessOverriderSystem : EntitySystem
    {
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly ILogManager _log = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!; // L5
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!; // L5

        public const string Sawmill = "accessoverrider";
        protected ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();
            _sawmill = _log.GetSawmill(Sawmill);

            SubscribeLocalEvent<AccessOverriderComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<AccessOverriderComponent, ComponentRemove>(OnComponentRemove);

            SubscribeLocalEvent<AccessOverriderComponent, GetVerbsEvent<UtilityVerb>>(OnGetVerbs); // L5
        }

        /// <summary>
        /// L5 - add verb. Slightly lazy way of doing this to minimize merge
        /// conflicts since I plan to do this upstream.
        /// </summary>
        private void OnGetVerbs(Entity<AccessOverriderComponent> ent, ref GetVerbsEvent<UtilityVerb> args)
        {
            if (!_interactionSystem.InRangeUnobstructed(args.User, args.Target)
                || !HasComp<AccessReaderComponent>(args.Target))
                return;

            var user = args.User;
            var target = args.Target;

            args.Verbs.Add(new UtilityVerb
            {
                Act = () => _doAfterSystem.TryStartDoAfter(
                    new DoAfterArgs(EntityManager,
                        user,
                        ent.Comp.DoAfter,
                        new AccessOverriderDoAfterEvent(),
                        ent,
                        target,
                        ent)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true,
                    }),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/in.svg.192dpi.png")),
                Text = Loc.GetString("door-electronics-configuration-title"),
            });
        }

        private void OnComponentInit(EntityUid uid, AccessOverriderComponent component, ComponentInit args)
        {
            _itemSlotsSystem.AddItemSlot(uid, AccessOverriderComponent.PrivilegedIdCardSlotId, component.PrivilegedIdSlot);
        }

        private void OnComponentRemove(EntityUid uid, AccessOverriderComponent component, ComponentRemove args)
        {
            _itemSlotsSystem.RemoveItemSlot(uid, component.PrivilegedIdSlot);
        }

        [Serializable, NetSerializable]
        public sealed partial class AccessOverriderDoAfterEvent : DoAfterEvent
        {
            public AccessOverriderDoAfterEvent()
            {
            }

            public override DoAfterEvent Clone() => this;
        }
    }
}

[ByRefEvent]
public record struct OnAccessOverriderAccessUpdatedEvent(EntityUid UserUid, bool Handled = false);
