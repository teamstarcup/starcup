using Content.Shared.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Speech.Components;

/// <summary>
/// Oi, speak ye peace or avast ye gob!
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CantAccentComponent : BaseAccentComponent;
