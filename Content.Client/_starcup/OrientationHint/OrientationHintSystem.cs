using Content.Shared._starcup.OrientationHint;
using Content.Shared.Examine;
using Robust.Shared.Map;

namespace Content.Client._starcup.OrientationHint;

public sealed class OrientationHintSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrientationHintComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<OrientationHintComponent> ent, ref ExaminedEvent args)
    {
        _entityManager.SpawnEntity(ent.Comp.ExamineArrow, new EntityCoordinates(ent, 0, 0));
    }
}
