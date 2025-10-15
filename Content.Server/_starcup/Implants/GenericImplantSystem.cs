using Content.Shared.Implants;
using Robust.Shared.Containers;

namespace Content.Server._starcup.Implants;

/// <summary>
/// Applies configured components to the implanted entity and, upon extraction, removes components which the entity did
/// not already have prior to implanting.
/// </summary>
public sealed class GenericImplantSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GenericImplantComponent, ImplantImplantedEvent>(OnImplantImplanted);
        SubscribeLocalEvent<GenericImplantComponent, EntGotRemovedFromContainerMessage>(OnImplantDraw);
    }

    private void OnImplantImplanted(Entity<GenericImplantComponent> entity, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted == EntityUid.Invalid)
        {
            return;
        }

        foreach (var component in entity.Comp.Components)
        {
            if (HasComp(ev.Implanted, component.GetType()))
                continue;

            var comp = _componentFactory.GetComponent(component.GetType());
            AddComp(ev.Implanted, comp);
            entity.Comp.RemoveOnExtract.Add(comp);
        }
    }

    private void OnImplantDraw(Entity<GenericImplantComponent> entity, ref EntGotRemovedFromContainerMessage ev)
    {
        foreach (var component in entity.Comp.RemoveOnExtract)
        {
            RemComp(ev.Container.Owner, component);
        }
        entity.Comp.RemoveOnExtract.Clear();
    }
}
