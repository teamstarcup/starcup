using Content.Server.Administration.Logs;
using Content.Server.Cargo.Components;
using Content.Server.Stack;
using Content.Shared._EE.Silicon.BlindHealing;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Tiles; // starcup

namespace Content.Server._EE.Silicon.BlindHealing;

public sealed class BlindHealingSystem : SharedBlindHealingSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly BlindableSystem _blindableSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BlindHealingComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<BlindHealingComponent, AfterInteractEvent>(OnInteract, before: [typeof(FloorTileSystem)]); // starcup: before floortile
        SubscribeLocalEvent<BlindHealingComponent, HealingDoAfterEvent>(OnHealingFinished);
    }

     private void OnHealingFinished(EntityUid uid, BlindHealingComponent component, HealingDoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null
            || !TryComp<BlindableComponent>(args.Target, out var blindComp)
            || blindComp is { EyeDamage: 0 })
            return;

        // begin starcup: stop misusing the item price
        if (TryComp<StackComponent>(uid, out var stackComponent) && component.StackCost > 0)
            _stackSystem.ReduceCount((uid, stackComponent), component.StackCost);
        // end starcup

        _blindableSystem.AdjustEyeDamage((args.Target.Value, blindComp), -blindComp.EyeDamage);

        _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(uid):target}'s vision");

        var str = Loc.GetString("comp-repairable-repair",
            ("target", args.User == args.Target ? Loc.GetString("verb-self-target-pronoun ") : args.Target), // starcup: actually say what we repaired, instead of the glass stack
            ("tool", args.Used!));
        _popup.PopupEntity(str, uid, args.User);

    }

    private bool TryHealBlindness(EntityUid uid, EntityUid user, EntityUid target, float delay)
    {
        var doAfterEventArgs =
            new DoAfterArgs(EntityManager, user, delay, new HealingDoAfterEvent(), uid, target: target, used: uid)
            {
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
            };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
        return true;
    }

    private void OnInteract(EntityUid uid, BlindHealingComponent component, ref AfterInteractEvent args)
    {

        if (args.Handled
            || !TryComp<DamageableComponent>(args.Target, out var damageable) // starcup: user -> target.
            || damageable.DamageContainerID != null && !component.DamageContainers.Contains(damageable.DamageContainerID)
            || !TryComp<BlindableComponent>(args.Target, out var blindcomp) // starcup: user -> target.
            || blindcomp.EyeDamage == 0
            || args.Target == null // starcup: we should have a target
            || args.User == args.Target && !component.AllowSelfHeal)
            return;

        args.Handled = TryHealBlindness(uid, args.User, args.Target.Value, // starcup: handle the event, set the target to the target
            args.User == args.Target
                ? component.DoAfterDelay * component.SelfHealPenalty
                : component.DoAfterDelay);
    }

    private void OnUse(EntityUid uid, BlindHealingComponent component, ref UseInHandEvent args)
    {
        if (args.Handled
            || !TryComp<DamageableComponent>(args.User, out var damageable)
            || damageable.DamageContainerID != null && !component.DamageContainers.Contains(damageable.DamageContainerID)
            || !TryComp<BlindableComponent>(args.User, out var blindcomp)
            || blindcomp.EyeDamage == 0
            || !component.AllowSelfHeal)
            return;

        args.Handled = TryHealBlindness(uid, args.User, args.User, // starcup: handle the event
            component.DoAfterDelay * component.SelfHealPenalty);
    }
}
