using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.BuildXenoWall;

/// <summary>
/// Component that allows xenos to spawn walls.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BuildXenoWallComponent : Component
{
    /// <summary>
    /// The action to add to the entity to spawn a wall.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionResinWall";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}
