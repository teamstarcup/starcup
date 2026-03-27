using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.BurrowAbility;

/// <summary>
/// Component that allows entities, such as animals, to burrow into the ground.
/// It works by polymorphing the entity into one of two entities, a small or large "burrowed creature".
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BurrowAbilityComponent : Component
{
    /// <summary>
    /// The action to add to the entity to allow them to burrow.
    ///
    /// The action used should result in a <see cref="PolymorphActionEvent"/> whose protoid is either BurrowedSmall or
    /// BurrowedLarge.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionBurrowSmall";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}
