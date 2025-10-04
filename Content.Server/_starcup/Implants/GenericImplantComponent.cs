using Robust.Shared.GameStates;

namespace Content.Server._starcup.Implants;

/// <summary>
/// Applies generic components to the implanted entity. Extracting the implant only removes those components which were
/// newly-added.
///
/// NOTE: You cannot specify non-default field values for the given components.
/// </summary>
[RegisterComponent]
public sealed partial class GenericImplantComponent : Component
{
    /// <summary>
    /// A list of components to apply to the implanted entity.
    /// </summary>
    /// <remarks>
    /// These objects are not directly copied to the entity for lifecycle reasons. Instead, new ones are created and
    /// those are the ones which are applied and removed.
    /// </remarks>
    [DataField(required: true)]
    public List<Component> Components = [];

    /// <summary>
    /// Used for tracking which components were actually added to this entity.
    /// </summary>
    public readonly List<IComponent> RemoveOnExtract = [];
}
