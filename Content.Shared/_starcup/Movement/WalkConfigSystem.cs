using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Content.Shared.Movement.Components;
using Content.Shared._starcup.CCVars;
using Content.Shared.Movement.Systems;
using Robust.Shared.Serialization;
using Content.Shared.Mind.Components;

namespace Content.Shared._starcup.Movement;

public sealed partial class InputMoverSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly INetConfigurationManager _netCfg = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private const MoveButtons WhenWalkPressed = MoveButtons.Walk;
    private const MoveButtons WhenWalkReleased = MoveButtons.None;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InputMoverComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<InputMoverComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeNetworkEvent<WalkByDefaultEvent>(OnWalkByDefaultEvent);
        Subs.CVar(_cfg, SCCVars.WalkByDefault, OnWalkByDefaultChanged);
    }

    private void OnMindAdded(Entity<InputMoverComponent> ent, ref MindAddedMessage args)
    {
        var userId = args.Mind.Comp.UserId;
        if (userId == null)
            return;

        if (!_playerMan.TryGetSessionById(userId.Value, out var session))
            return;

        ent.Comp.SprintsWhen = _netCfg.GetClientCVar(session.Channel, SCCVars.WalkByDefault) ? WhenWalkPressed : WhenWalkReleased;
    }

    private void OnMindRemoved(Entity<InputMoverComponent> ent, ref MindRemovedMessage args)
    {
        // Reset to default
        ent.Comp.SprintsWhen = WhenWalkReleased;
    }

    private void OnWalkByDefaultEvent(WalkByDefaultEvent evt, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } entity)
            return;

        if (!TryComp<InputMoverComponent>(entity, out var mover))
            return;

        mover.SprintsWhen = _netCfg.GetClientCVar(args.SenderSession.Channel, SCCVars.WalkByDefault) ? WhenWalkPressed : WhenWalkReleased;
    }

    private void OnWalkByDefaultChanged(bool enabled)
    {
        if (_playerMan.LocalEntity == null)
            return;

        RaiseNetworkEvent(new WalkByDefaultEvent());
    }
}

[Serializable, NetSerializable]
public sealed class WalkByDefaultEvent() : EntityEventArgs
{
}
