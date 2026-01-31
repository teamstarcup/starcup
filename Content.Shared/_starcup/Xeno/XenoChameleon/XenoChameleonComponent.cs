using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.XenoChameleon;

/// <summary>
/// Component that allows a xeno to become translucent for a time.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenoChameleonComponent : Component
{
    /// <summary>
    /// The action to add to the entity polymorph into its translucently sprited self.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionXenoChameleonSkulker";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}
