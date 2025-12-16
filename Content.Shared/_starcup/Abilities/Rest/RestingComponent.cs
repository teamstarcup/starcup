using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Abilities.Rest;

[RegisterComponent, NetworkedComponent]
[Access(typeof(RestSystem))]
public sealed partial class RestingComponent : Component
{
}
