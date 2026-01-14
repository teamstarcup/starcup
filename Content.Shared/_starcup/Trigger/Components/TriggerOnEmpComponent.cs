using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Trigger.Components;

/// <summary>
/// Triggers an entity when someone gets hit by an EMP pulse.
/// The user is the entity that was hit by the EMP.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnEmpComponent : BaseTriggerOnXComponent;
