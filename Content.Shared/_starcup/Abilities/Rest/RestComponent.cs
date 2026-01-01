using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._starcup.Abilities.Rest;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RestComponent : Component {
    [DataField]
    public EntityUid? RestActionEntity;
    /// <summary>
    /// Layer and sprite state to use when resting.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> RestingLayers = new();

    /// <summary>
    /// Layer and sprite state to use when not resting.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> NotRestingLayers = new();

    [DataField, AutoNetworkedField]
    public bool IsResting;
    }
