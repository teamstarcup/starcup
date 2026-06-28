using System.Numerics;
using Content.Shared.Body;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Body;

[UsedImplicitly]
public sealed partial class OrganReplacementSpecial : JobSpecial
{
    [DataField(required: true)]
    public Dictionary<ProtoId<OrganCategoryPrototype>, EntProtoId<OrganComponent>> Organs;

    public override void AfterEquip(EntityUid mob)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var containerSystem = entityManager.System<SharedContainerSystem>();

        if (!entityManager.TryGetComponent<ContainerManagerComponent>(mob, out var containerComp))
            return;

        if (entityManager.IsQueuedForDeletion(mob) || !entityManager.EntityExists(mob))
            return;

        if (!containerSystem.TryGetContainer(mob, BodyComponent.ContainerID, out var container, containerComp))
            return;

        var organsToRemove = new List<EntityUid>();
        foreach (var organUid in container.ContainedEntities)
        {
            if (!entityManager.TryGetComponent<OrganComponent>(organUid, out var organComponent))
                continue;

            if (organComponent.Category is null || !Organs.ContainsKey(organComponent.Category.Value.Id))
                continue;

            organsToRemove.Add(organUid);
        }

        organsToRemove.ForEach(organ => entityManager.DeleteEntity(organ));

        var coords = new EntityCoordinates(mob, Vector2.Zero);
        foreach (var pair in Organs)
        {
            // TODO(space-wizards/RobustToolbox#6192): TrySpawnInContainer
            var newOrgan = entityManager.SpawnAttachedTo(pair.Value.Id, coords);
            if (!containerSystem.Insert(newOrgan, container))
                entityManager.DeleteEntity(newOrgan);
        }
    }
}
