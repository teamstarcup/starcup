using Content.Shared.Bed.Sleep;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Resting;

/// <summary>
/// Component that allows entities to sleep at any point, without needing a bed.
/// Useful for cats and other animals that nap wherever
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RestComponent : Component
{
    /// <summary>
    /// The action to add to the entity to allow them to sleep.
    ///
    /// The action used should result in a <see cref="SleepActionEvent"/>
    /// </summary>
    [DataField]
    public EntProtoId RestAction = SleepingSystem.SleepActionId;

    [DataField, AutoNetworkedField]
    public EntityUid? RestActionEntity;

    /// <summary>
    /// Layer and sprite state to use when asleep.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> SleepingLayers = new();

    /// <summary>
    /// Layer and sprite state to use when awake.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> AwakeLayers = new();
}
