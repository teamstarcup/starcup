using System.Numerics;
using Content.Server.Decals;
using Content.Shared._starcup.Tools.Systems;
using Content.Shared.Decals;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._starcup.Tools.Systems;

public sealed class ScrubTileToolSystem : SharedScrubTileToolSystem
{
    [Dependency] private readonly DecalSystem _decalSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    public override bool TryDoScrub(TileRef tileRef, MapGridComponent grid, DecalGridComponent decalGrid)
    {
        var bounds = _lookupSystem.GetLocalBounds(tileRef, grid.TileSize).Translated(new Vector2(-0.5f, -0.5f));
        var decals = _decalSystem.GetDecalsIntersecting(tileRef.GridUid, bounds);

        foreach (var decal in decals)
        {
            if (decal.Decal.Cleanable)
                _decalSystem.RemoveDecal(tileRef.GridUid, decal.Index, decalGrid);
        }
        return true;
    }
}
