using Content.Shared.Actions;

namespace Content.Shared._starcup.BuildXenoWall;

public sealed class BuildXenoWallSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BuildXenoWallComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BuildXenoWallComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }
}
