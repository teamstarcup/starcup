using Content.Shared.Alert;
using Content.Shared.Body;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.MKC;

/// <summary>
/// starcup: An electricity-based hunger analogue for MKCs. Players with the power core organ have an internal battery
/// that they need to keep charged to avoid hunger-like effects. They are able to replenish charge by draining anything
/// with BatteryComponent.
/// </summary>
public abstract class SharedPowerCoreSystem : EntitySystem
{
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly INetManager _net = default!;

    private const float MaxEnergyDrainDistance = 32.0f;
    private readonly SoundSpecifier? _drainSounds = new SoundCollectionSpecifier("sparks");
    private readonly ProtoId<AlertPrototype> _batteryAlertPrototype = "BorgBattery";

    public override void Initialize()
    {
        base.Initialize();

        // TODO: Localization

        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<GetVerbsEvent<InnateVerb>>>(AddEnergyDrainVerb);
        SubscribeLocalEvent<PowerCoreComponent, PowerCoreDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<LocalPlayerAttachedEvent>>(OnPlayerAttached);
        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<LocalPlayerDetachedEvent>>(OnPlayerDetached);

        // SubscribeLocalEvent<PowerCoreComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // TODO: Drains power over time [on par with hunger]
    }

    /// <summary>
    /// Returns the entity, if any, which contains this organ.
    /// </summary>
    /// <param name="powerCore"></param>
    /// <returns></returns>
    private EntityUid? GetUsingMob(Entity<PowerCoreComponent> powerCore)
    {
        OrganComponent? organ = null;
        return !Resolve(powerCore.Owner, ref organ) ? null : organ.Body;
    }

    // TODO: Clear alert if organ is removed from player
    // private void OnShutdown(Entity<PowerCoreComponent> powerCore, ref ComponentShutdown args)
    // {
    //     var bodyUid = GetUsingMob(powerCore);
    //     if (bodyUid == null)
    //         return;
    //
    //     _alerts.ClearAlertCategory(bodyUid.Value, "Battery");
    // }

    private void UpdateMoveSpeedModifier(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        Entity<BatteryComponent?> battery = (powerCore.Owner, null);
        if (!Resolve(powerCore.Owner, ref battery.Comp))
            return;

        if (_battery.GetCharge(battery) > 0)
            return;

        // No slowdown in weightlessness
        if (_jetpack.IsUserFlying(args.Body.Owner))
             return;

        args.Args.ModifySpeed(powerCore.Comp.LowPowerMovementSpeedMultiplier, powerCore.Comp.LowPowerMovementSpeedMultiplier);
    }

    private void OnPlayerAttached(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<LocalPlayerAttachedEvent> args)
    {
        UpdateBatteryAlert(args.Body.Owner, powerCore);
    }

    private void OnPlayerDetached(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<LocalPlayerDetachedEvent> args)
    {
        _alerts.ClearAlert(args.Args.Entity, _batteryAlertPrototype);
    }

    protected void UpdateBatteryAlert(EntityUid body, Entity<PowerCoreComponent> powerCore)
    {
        var battery = new Entity<BatteryComponent?>(powerCore.Owner, null);
        if (!Resolve(powerCore.Owner, ref battery.Comp))
            return;

        if (!TryComp(body, out AlertsComponent? alerts))
            return;

        // alert levels from 0 to 10
        var chargeLevel = _battery.GetChargeLevel(battery);
        var alertLevel = (int) MathF.Round(chargeLevel * 10f);
        alertLevel = chargeLevel > 0 ? Math.Max(alertLevel, 1) : 0;

        _alerts.ShowAlert((body, alerts), _batteryAlertPrototype, (short) alertLevel);
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
            Text = Loc.GetString("stethoscope-verb"),
            IconEntity = GetNetEntity(powerCore),
            Priority = 2,
        };
        args.Args.Verbs.Add(verb);
    }

    private void StartDraining(Entity<PowerCoreComponent> powerCore, EntityUid target)
    {
        var user = GetUsingMob(powerCore);
        if (user == null)
            return;

        Entity<BatteryComponent?> powerCoreBattery = (powerCore.Owner, null);
        if (!Resolve(powerCoreBattery, ref powerCoreBattery.Comp))
            return;

        Entity<BatteryComponent?> targetBattery = (target, null);
        if (!Resolve(targetBattery, ref targetBattery.Comp))
            return;

        if (_battery.IsFull(powerCoreBattery))
        {
            _popup.PopupPredicted("Your battery is already full.", user.Value, user.Value);
            return;
        }

        if (_battery.GetCharge(targetBattery) <= 1.0f && (!targetBattery.Comp.NetSyncEnabled && _net.IsClient))
        {
            _popup.PopupPredicted("The battery is empty.", user.Value, user.Value);
            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user.Value, powerCore.Comp.Delay, new PowerCoreDoAfterEvent(), powerCore, target: target, used: powerCore)
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
        var targetBatteryIsEmpty = _battery.GetCharge(targetBattery) <= 1.0f;

        args.Repeat = !powerCoreBatteryIsFull && !targetBatteryIsEmpty;
    }

    private void Drink(Entity<PowerCoreComponent> powerCore, Entity<BatteryComponent?> target)
    {
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
        {
            _popup.PopupPredicted("There is nothing left to drain.", powerCore.Owner, powerCore.Owner);
            return;
        }

        _battery.SetCharge(powerCoreBattery, powerCoreBatteryCharge + joulesToDrain);
        _battery.SetCharge(target, targetBatteryCharge - joulesToDrain);

        _audio.PlayPvs(_drainSounds, target.Owner);
        Spawn("EffectSparks", Transform(target.Owner).Coordinates);

        _popup.PopupPredicted("You drain the battery of some power.", powerCore.Owner, powerCore.Owner);
    }
}
