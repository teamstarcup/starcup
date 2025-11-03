using Content.Shared.Construction;
using Content.Shared.Construction.Conditions;
using Content.Shared.Maps;
using Robust.Shared.Map;

namespace Content.Shared._starcup.Construction.Conditions;

/// <summary>
/// starcup: Requires that the construction must be placed on a subfloor tile.
/// </summary>
[DataDefinition]
public sealed partial class TileSubfloorCondition : IConstructionCondition
{
    public ConstructionGuideEntry GenerateGuideEntry()
    {
        return new ConstructionGuideEntry
        {
            Localization = "construction-step-condition-tile-is-subfloor",
        };
    }

    public bool Condition(EntityUid user, EntityCoordinates location, Direction direction)
    {
        if (!IoCManager.Resolve<IEntityManager>().TrySystem<TurfSystem>(out var turfSystem))
            return false;

        if (!turfSystem.TryGetTileRef(location, out var tileRef))
            return false;

        var tileDef = turfSystem.GetContentTileDefinition(tileRef.Value);
        return tileDef.IsSubFloor;
    }
}
