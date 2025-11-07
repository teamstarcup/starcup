using Content.Shared._starcup.Tools.Components;
using Content.Shared.Decals;
using Content.Shared.Fluids.Components;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._starcup.Tools.Systems;

public abstract class SharedScrubTileToolSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly TurfSystem _turfs = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;

    private const string ScrubQuality = "Brushing";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ScrubbingToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<ScrubbingToolComponent, TileToolDoAfterEvent>(OnScrubbingComplete);
    }

    private void OnAfterInteract(Entity<ScrubbingToolComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target != null && !HasComp<PuddleComponent>(args.Target))
            return;

        args.Handled = TryStartScrubbing(ent, args.User, args.ClickLocation);
    }

    private bool TryStartScrubbing(Entity<ScrubbingToolComponent> ent, EntityUid user, EntityCoordinates clickedCoords)
    {
        if (!TryComp<ToolComponent>(ent, out var tool))
            return false;

        if (!HasScrubQuality(tool))
            return false;

        if (!_mapManager.TryFindGridAt(_transformSystem.ToMapCoordinates(clickedCoords), out var gridUid, out var mapGrid))
            return false;

        var tileRef = _maps.GetTileRef(gridUid, mapGrid, clickedCoords);

        if (ent.Comp.RequiresUnobstructed && _turfs.IsTileBlocked(gridUid, tileRef.GridIndices, CollisionGroup.MobMask))
            return false;

        var coordinates = _maps.GridTileToLocal(gridUid, mapGrid, tileRef.GridIndices);
        if (!_interactionSystem.InRangeUnobstructed(user, coordinates, popup: false))
            return false;

        var args = new TileToolDoAfterEvent(GetNetEntity(gridUid), tileRef.GridIndices);
        _toolSystem.UseTool(ent, user, ent, ent.Comp.Delay, tool.Qualities, args, out _, toolComponent: tool);
        return true;
    }

    private void OnScrubbingComplete(Entity<ScrubbingToolComponent> ent, ref TileToolDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!TryComp<ToolComponent>(ent, out var tool))
            return;

        if (!HasScrubQuality(tool))
            return;

        var gridUid = GetEntity(args.Grid);
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
        {
            Log.Error("Attempted to scrub a tile on a non-existant grid?");
            return;
        }
        if (!TryComp<DecalGridComponent>(gridUid, out var decalGrid))
            return;

        var tileRef = _maps.GetTileRef(gridUid, grid, args.GridTile);

        if (!TryDoScrub(tileRef, grid, decalGrid))
            return;

        args.Handled = true;
    }

    private static bool HasScrubQuality(ToolComponent tool)
    {
        // Using qualities directly gives an access error
        // But assigning it to a variable first works
        var qualities = tool.Qualities;
        return qualities.Contains(ScrubQuality);
    }

    public virtual bool TryDoScrub(TileRef tileRef, MapGridComponent grid, DecalGridComponent decalGrid)
    {
        // Don't bother on the client, decals only remove on the server
        return true;
    }
}
