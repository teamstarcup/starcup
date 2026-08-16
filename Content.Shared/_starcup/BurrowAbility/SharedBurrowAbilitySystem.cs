using Content.Shared.Actions;

namespace Content.Shared._starcup.BurrowAbility;

public sealed partial class BurrowAbilitySystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BurrowAbilityComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BurrowAbilityComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }
}
