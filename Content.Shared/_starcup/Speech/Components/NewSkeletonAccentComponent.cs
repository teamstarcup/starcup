using Content.Shared._starcup.Speech.EntitySystems;
using Content.Shared.Speech.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Speech.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(NewSkeletonAccentSystem))]
public sealed partial class NewSkeletonAccentComponent : BaseAccentComponent;
