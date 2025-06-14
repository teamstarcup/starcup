using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.OrientationHint;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class OrientationHintComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId ExamineArrow = "TurnstileArrow";

    /// <summary>
    ///     The direction in which the examine arrow is facing, relative to the entity's rotation. Defaults to south.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public Angle Direction = Angle.Zero;
}
