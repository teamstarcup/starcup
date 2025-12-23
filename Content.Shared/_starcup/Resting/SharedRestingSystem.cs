using Content.Shared.Actions;

namespace Content.Shared._starcup.Resting;

public abstract partial class SharedRestingSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RestComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(Entity<RestComponent> ent, ref ComponentInit args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.RestActionEntity, ent.Comp.RestAction);
    }
}
