using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Traits;

/// <summary>
/// Used for characters who cannot be cloned, but can be revived through other means.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UncloneableComponent : Component;
