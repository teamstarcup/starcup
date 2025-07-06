using System.Numerics;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Server.Standing;

public sealed class StandingStateSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    // begin starcup: rewritten for hands refactor
    private void FallOver(EntityUid uid, StandingStateComponent component, DropHandItemsEvent args)
    {
        var direction = EntityManager.TryGetComponent(uid, out PhysicsComponent? comp) ? comp.LinearVelocity / 50 : Vector2.Zero;
        var dropAngle = _random.NextFloat(0.8f, 1.2f);

        var fellEvent = new FellDownEvent(uid);
        RaiseLocalEvent(uid, fellEvent, false);

        if (!TryComp(uid, out HandsComponent? handsComp))
            return;

        var worldRotation = _transformSystem.GetWorldRotation(uid).ToVec();
        foreach (var hand in handsComp.Hands.Keys)
        {
            var handEntity = new Entity<HandsComponent?>(uid, handsComp);

            if (_handsSystem.GetHeldItem(new Entity<HandsComponent?>(uid, handsComp), hand) is not EntityUid held)
                continue;

            if (!_handsSystem.TryDrop(uid, handEntity, null, checkActionBlocker: false))
                continue;

            _throwingSystem.TryThrow(held,
                _random.NextAngle().RotateVec(direction / dropAngle + worldRotation / 50),
                0.5f * dropAngle * _random.NextFloat(-0.9f, 1.1f),
                uid, 0);
        }
    }
    // end starcup

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StandingStateComponent, DropHandItemsEvent>(FallOver);
    }

}

/// <summary>
/// Raised after an entity falls down.
/// </summary>
public sealed class FellDownEvent : EntityEventArgs
{
    public EntityUid Uid { get; }
    public FellDownEvent(EntityUid uid)
    {
        Uid = uid;
    }
}
