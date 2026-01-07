using Content.Shared.Actions;

namespace Content.Shared._starcup.BurrowAbility;

public abstract partial class SharedBurrowAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BurrowAbilityComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BurrowAbilityComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.BurrowAbilityActionEntity, ent.Comp.BurrowAbilityAction);
    }
}
