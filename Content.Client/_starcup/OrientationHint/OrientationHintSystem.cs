using Content.Shared._starcup.OrientationHint;
using Content.Shared.Examine;
using Robust.Client.GameObjects;
using Robust.Shared.Map;

namespace Content.Client._starcup.OrientationHint;

public sealed class OrientationHintSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrientationHintComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<OrientationHintComponent> ent, ref ExaminedEvent args)
    {
        var arrowEntity = EntityManager.SpawnEntity(ent.Comp.ExamineArrow, new EntityCoordinates(ent, 0, 0));

        TransformComponent? arrowTransform = default!;
        if (!Resolve<TransformComponent>(arrowEntity, ref arrowTransform))
            return;

        _transform.SetLocalRotationNoLerp(arrowEntity, ent.Comp.Direction, arrowTransform);
    }
}
