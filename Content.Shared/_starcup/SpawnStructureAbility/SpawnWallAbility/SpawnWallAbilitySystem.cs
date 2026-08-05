using Content.Shared.Actions;
using Content.Shared.Cloning.Events;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Serialization;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Content.Shared.Nutrition.Components;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared.SpawnWallAbility;

/// <summary>
/// This is a modified duplicate of SericultureSystem.
/// Allows mobs to spawn structures with <see cref="SpawnWallAbilityComponent"/>.
/// </summary>
public abstract partial class SharedSpawnWallAbilitySystem : EntitySystem
{
    // Managers
    [Dependency] private readonly INetManager _netManager = default!;

    // Systems
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly HungerSystem _hungerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStackSystem _stackSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!; // starcup audio stuff
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnWallAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpawnWallAbilityComponent, ComponentShutdown>(OnCompRemove);
        SubscribeLocalEvent<SpawnWallAbilityComponent, SpawnWallAbilityActionEvent>(OnSpawnWallAbilityStart);
        SubscribeLocalEvent<SpawnWallAbilityComponent, SpawnWallAbilityDoAfterEvent>(OnSpawnWallAbilityDoAfter);
        SubscribeLocalEvent<SpawnWallAbilityComponent, CloningEvent>(OnClone);
    }

    private void OnClone(Entity<SpawnWallAbilityComponent> ent, ref CloningEvent args)
    {
        if (!args.Settings.EventComponents.Contains(Factory.GetRegistration(ent.Comp.GetType()).Name))
            return;

        // Make sure to set the datafields before adding the component so that the correct action gets spawned on map init.
        var cloneComp = Factory.GetComponent<SpawnWallAbilityComponent>();
        cloneComp.PopupText = ent.Comp.PopupText;
        cloneComp.EntityProduced = ent.Comp.EntityProduced;
        cloneComp.Action = ent.Comp.Action;
        cloneComp.ProductionLength = ent.Comp.ProductionLength;
        cloneComp.HungerCost = ent.Comp.HungerCost;
        cloneComp.MinHungerThreshold = ent.Comp.MinHungerThreshold;
        AddComp(args.CloneUid, cloneComp, true);
    }

    /// <summary>
    /// Gives the action to preform the spawning on the entity
    /// </summary>
    private void OnMapInit(EntityUid uid, SpawnWallAbilityComponent comp, MapInitEvent args)
    {
        _actionsSystem.AddAction(uid, ref comp.ActionEntity, comp.Action);
    }

    /// <summary>
    /// Takes away the action to preform the spawning from the entity.
    /// </summary>
    private void OnCompRemove(EntityUid uid, SpawnWallAbilityComponent comp, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, comp.ActionEntity);
    }

    private void OnSpawnWallAbilityStart(EntityUid uid, SpawnWallAbilityComponent comp, SpawnWallAbilityActionEvent args)
    {
        if (!TryComp<HungerComponent>(uid, out var hungerComp)
            || _hungerSystem.IsHungerBelowState(uid,
                comp.MinHungerThreshold,
                _hungerSystem.GetHunger(hungerComp) - comp.HungerCost,
                hungerComp))
        {
            _popupSystem.PopupClient(Loc.GetString(comp.PopupText), uid, uid);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, uid, comp.ProductionLength, new SpawnWallAbilityDoAfterEvent(), uid)
        {
            // I'm not sure if more things should be put here, but imo ideally it should probably be set in the component/YAML. Not sure if this is currently possible.
            BreakOnMove = true,
            BlockDuplicate = true,
            BreakOnDamage = true,
            CancelDuplicate = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfter);

        if (!_timing.IsFirstTimePredicted)
            return;

        _audio.Stop(comp.SpawnStream);
        comp.SpawnStream = _audio.PlayPredicted(comp.SoundProgress, uid, uid)?.Entity; // starcup audio stuff
    }

    private EntityCoordinates? GetForwardTile(TransformComponent transform)
    {
        var directionPos = transform.Coordinates.Offset(transform.LocalRotation.ToWorldVec().Normalized());

        if (!TryComp<MapGridComponent>(transform.GridUid, out var mapGrid))
            return null;
        if (!_turf.TryGetTileRef(directionPos, out var tileReference))
            return null;

        var tileIndex = tileReference.Value.GridIndices;
        return _mapSystem.GridTileToLocal(transform.GridUid.Value, mapGrid, tileIndex);
    }

    private void OnSpawnWallAbilityDoAfter(EntityUid uid, SpawnWallAbilityComponent comp, SpawnWallAbilityDoAfterEvent args)
    {
        if (_timing.IsFirstTimePredicted)
            comp.SpawnStream = _audio.Stop(comp.SpawnStream); // starcup audio stuff

        if (args.Cancelled || args.Handled || comp.Deleted)
            return;

        if (!TryComp<HungerComponent>(uid,
                out var hungerComp) // A check, just incase the doafter is somehow performed when the entity is not in the right hunger state.
            || _hungerSystem.IsHungerBelowState(uid,
                comp.MinHungerThreshold,
                _hungerSystem.GetHunger(hungerComp) - comp.HungerCost,
                hungerComp))
        {
            _popupSystem.PopupClient(Loc.GetString(comp.PopupText), uid, uid);
            return;
        }

        _hungerSystem.ModifyHunger(uid, -comp.HungerCost, hungerComp);

        _audio.PlayPredicted(comp.SoundFinished, uid, uid); // starcup audio stuff

        if (!_netManager.IsClient) // Have to do this because spawning stuff in shared is CBT.
        {
            var coordinates = GetForwardTile(Transform(uid));
            if (coordinates is not null)
            {
                Spawn(comp.EntityProduced, coordinates.Value);
            }
        }
        args.Repeat = true;
    }
}

/// <summary>
/// Should be relayed upon using the action.
/// </summary>
public sealed partial class SpawnWallAbilityActionEvent : InstantActionEvent { }

/// <summary>
/// Is relayed at the end of the spawning doafter.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SpawnWallAbilityDoAfterEvent : SimpleDoAfterEvent { }

