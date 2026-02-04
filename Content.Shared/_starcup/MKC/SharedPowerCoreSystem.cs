using Content.Shared.Body;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

// ReSharper disable InconsistentNaming

namespace Content.Shared._starcup.MKC;

/// <summary>
/// starcup: An electricity-based hunger analogue for MKCs. Players with the power core organ have an internal battery
/// that they need to keep charged to avoid hunger-like effects. They are able to replenish charge by draining anything
/// with BatteryComponent.
/// </summary>
public abstract class SharedPowerCoreSystem : EntitySystem
{
    [Dependency] protected readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float MaxEnergyDrainDistance = 32.0f;
    private readonly SoundSpecifier? _drainSounds = new SoundCollectionSpecifier("sparks");

    public override void Initialize()
    {
        base.Initialize();

        // TODO: Call SharedBatterySystem.RefreshChargeRate when this organ is removed from the body

        SubscribeLocalEvent<PowerCoreComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<GetVerbsEvent<InnateVerb>>>(AddEnergyDrainVerb);
        SubscribeLocalEvent<PowerCoreComponent, PowerCoreDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<PowerCoreComponent, RefreshChargeRateEvent>(OnRefreshChargeRate);
        SubscribeLocalEvent<PowerCoreComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);
        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<RefreshMovementSpeedModifiersEvent>>(UpdateMoveSpeedModifier);
    }

    private void OnMapInit(Entity<PowerCoreComponent> ent, ref MapInitEvent _)
    {
        Entity<BatteryComponent?> battery = (ent.Owner, null);
        if (!Resolve(battery, ref battery.Comp))
            return;

        _battery.RefreshChargeRate(battery);

        var body = GetContainingBody(ent);
        if (body == null)
            return;

        _movementSpeed.RefreshMovementSpeedModifiers(body.Value);
    }

    private void OnRefreshChargeRate(Entity<PowerCoreComponent> powerCore, ref RefreshChargeRateEvent args)
    {
        // TODO: Should not drain power while body is 'dead'
        // TODO: Should not drain power while not inside a body
        // TODO: Maybe drain less power while sleeping? Call SharedBatterySystem.RefreshChargeRate when conditions that modify this value change
        args.NewChargeRate -= powerCore.Comp.WattConsumption;
    }

    private void OnBatteryStateChanged(Entity<PowerCoreComponent> powerCore, ref BatteryStateChangedEvent args)
    {
        var uid = GetContainingBody(powerCore);
        if (uid == null)
            return;

        if (args.NewState == BatteryState.Empty || args.OldState == BatteryState.Empty)
            _movementSpeed.RefreshMovementSpeedModifiers(uid.Value);
    }

    /// <summary>
    /// Returns the entity, if any, which contains this organ.
    /// </summary>
    /// <param name="powerCore"></param>
    /// <returns></returns>
    private EntityUid? GetContainingBody(Entity<PowerCoreComponent> powerCore)
    {
        OrganComponent? organ = null;
        return !Resolve(powerCore.Owner, ref organ) ? null : organ.Body;
    }

    private void UpdateMoveSpeedModifier(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        // TODO: Call _movementSpeed.RefreshMovementSpeedModifiers on the body entity when the organ is removed from it

        Entity<BatteryComponent?> battery = (powerCore.Owner, null);
        if (!Resolve(powerCore.Owner, ref battery.Comp))
            return;

        if (!MathHelper.CloseToPercent(_battery.GetCharge(battery), 0))
            return;

        // No slowdown in weightlessness
        if (_jetpack.IsUserFlying(args.Body.Owner))
             return;

        args.Args.ModifySpeed(powerCore.Comp.LowPowerMovementSpeedMultiplier);
    }

    private void AddEnergyDrainVerb(Entity<PowerCoreComponent> powerCore,
        ref BodyRelayedEvent<GetVerbsEvent<InnateVerb>> args)
    {
        if (!args.Args.CanInteract || !args.Args.CanAccess)
            return;

        if (!HasComp<BatteryComponent>(args.Args.Target))
            return;

        var target = args.Args.Target;

        InnateVerb verb = new()
        {
            Act = () => StartDraining(powerCore, target),
            Text = Loc.GetString("power-core-verb"),
            IconEntity = GetNetEntity(powerCore),
            Priority = 2,
        };
        args.Args.Verbs.Add(verb);
    }

    private void StartDraining(Entity<PowerCoreComponent> powerCore, EntityUid target)
    {
        var bodyUid = GetContainingBody(powerCore);
        if (bodyUid == null)
            return;

        Entity<BatteryComponent?> powerCoreBattery = (powerCore.Owner, null);
        if (!Resolve(powerCoreBattery, ref powerCoreBattery.Comp))
            return;

        Entity<BatteryComponent?> targetBattery = (target, null);
        if (!Resolve(targetBattery, ref targetBattery.Comp))
            return;

        if (_battery.IsFull(powerCoreBattery))
        {
            _popup.PopupClient(
                Loc.GetString("power-core-full", ("organ", powerCore)),
                bodyUid.Value,
                bodyUid.Value
                );
            return;
        }

        if (MathHelper.CloseToPercent(_battery.GetCharge(targetBattery), 0) && targetBattery.Comp.NetSyncEnabled)
        {
            _popup.PopupClient(
                Loc.GetString("power-core-battery-empty", ("target", target)),
                bodyUid.Value,
                bodyUid.Value
                );
            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, bodyUid.Value, powerCore.Comp.Delay, new PowerCoreDoAfterEvent(), powerCore, target: target, used: powerCore)
        {
            DuplicateCondition = DuplicateConditions.SameEvent,
            BreakOnMove = true,
            BreakOnHandChange = false,
            BreakOnDamage = true,
            MovementThreshold = 0.01f,
            DistanceThreshold = MaxEnergyDrainDistance,
        });
    }

    private void OnDoAfter(Entity<PowerCoreComponent> powerCore, ref PowerCoreDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not {} target)
            return;

        Entity<BatteryComponent?> powerCoreBattery = (powerCore.Owner, null);
        if (!Resolve(powerCoreBattery, ref powerCoreBattery.Comp))
            return;

        Entity<BatteryComponent?> targetBattery = (target, null);
        if (!Resolve(targetBattery, ref targetBattery.Comp))
            return;

        Drink(powerCore, targetBattery);

        var powerCoreBatteryIsFull = _battery.IsFull(powerCoreBattery);
        var targetBatteryIsEmpty = MathHelper.CloseToPercent(_battery.GetCharge(targetBattery), 0);

        args.Repeat = !powerCoreBatteryIsFull && !targetBatteryIsEmpty;
    }

    private void Drink(Entity<PowerCoreComponent> powerCore, Entity<BatteryComponent?> target)
    {
        var body = GetContainingBody(powerCore);
        if (body == null)
            return;

        if (target.Comp == null && !Resolve(target.Owner, ref target.Comp))
            return;

        var powerCoreBattery = new Entity<BatteryComponent?>(powerCore.Owner, null);
        if (!Resolve(powerCore.Owner, ref powerCoreBattery.Comp))
            return;

        var powerCoreBatteryCharge = _battery.GetCharge(powerCoreBattery);
        var targetBatteryCharge = _battery.GetCharge(target);

        var joulesNeeded = Math.Max(powerCoreBattery.Comp.MaxCharge - powerCoreBatteryCharge, 0);
        var joulesToDrain = Math.Min(targetBatteryCharge, joulesNeeded);
        joulesToDrain = Math.Min(joulesToDrain, powerCore.Comp.JoulesPerDrain);

        if (joulesToDrain <= 0f)
            return;

        _battery.SetCharge(powerCoreBattery, powerCoreBatteryCharge + joulesToDrain);
        _battery.SetCharge(target, targetBatteryCharge - joulesToDrain);

        // TODO: Check if prediction still causes this to fire multiple times when draining held items
        _audio.PlayPredicted(_drainSounds, target.Owner, body);
        if (_timing.IsFirstTimePredicted)
            Spawn("EffectSparks", Transform(target.Owner).Coordinates);

        _popup.PopupPredicted(
            Loc.GetString("power-core-drain", ("target", target)),
            powerCore.Owner,
            powerCore.Owner
            );
    }
}

