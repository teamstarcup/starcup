using Robust.Shared.GameStates;

namespace Content.Shared._starcup.MKC;

/// <summary>
/// starcup: Indicates an entity which can be drained of electricity to refill a power core.
/// </summary>
/// <remarks>
/// This is created to work around complexity with SharedInteractionSystem and event subscriptions.
/// </remarks>
[RegisterComponent, NetworkedComponent]
public sealed partial class PowerDrinkableComponent : Component
{

}
