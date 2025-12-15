using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Movement.Events;

namespace Content.Shared._starcup.Abilities.Rest;

public sealed class RestSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RestingComponent, UpdateCanMoveEvent>(OnRestingCanMove);
        SubscribeLocalEvent<RestComponent, ComponentInit>(OnComponentInit);
    }
    private void OnComponentInit(Entity<RestComponent> ent, ref ComponentInit args)
    {
        // The below line should get a warning, but we'll fix it in a bit
        _actionsSystem.AddAction(ent, ref ent.Comp.RestActionEntity, "ActionRest");
    }
    private void OnRestingCanMove(Entity<RestingComponent> ent, ref UpdateCanMoveEvent args)
    {
        args.Cancel();
    }

    private void OnXenoRest(Entity<RestComponent> ent, ref RestEvent args)
    {
        if (HasComp<RestingComponent>(ent))
        {
            RemComp<RestingComponent>(ent);
            _appearance.SetData(ent, RestVisualLayers.Base, RestState.NotResting);
        }
        else
        {
            AddComp<RestingComponent>(ent);
            _appearance.SetData(ent, RestVisualLayers.Base, RestState.Resting);
        }

        _actionBlocker.UpdateCanMove(ent);
    }
}
