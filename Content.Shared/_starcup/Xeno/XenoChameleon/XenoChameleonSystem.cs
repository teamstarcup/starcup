using Content.Shared.Actions;

namespace Content.Shared._starcup.XenoChameleon;

public sealed class XenoChameleonSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoChameleonComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<XenoChameleonComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }
}
