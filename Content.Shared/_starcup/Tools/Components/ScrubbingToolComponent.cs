using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Tools.Components;

/// <summary>
/// This is used for entities with <see cref="ToolComponent"/> that are also able
/// to remove decals.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ScrubbingToolComponent : Component
{
    /// <summary>
    /// The time it takes to scrub away the decals
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Whether or not the tile being scrubbed must be unobstructed
    /// </summary>
    [DataField]
    public bool RequiresUnobstructed = true;
}
