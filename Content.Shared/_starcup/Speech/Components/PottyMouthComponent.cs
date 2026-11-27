using Content.Shared.Speech.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Speech.Components;

/// <summary>
/// Replaces the speaker's swear words with minced oaths.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PottyMouthComponent : BaseAccentComponent;
