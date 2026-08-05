using Content.Shared.Nutrition.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Audio;

namespace Content.Shared.SpawnWindowAbility;

/// <summary>
/// This is a modified duplicate of SericultureComponent.
/// Should be applied to any mob that you want to be able to spawn a structure with an action and the cost of hunger.
/// TODO: Probably adjust this to utilize organs?
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpawnWindowAbilitySystem)), AutoGenerateComponentState]
public sealed partial class SpawnWindowAbilityComponent : Component
{
    /// <summary>
    /// The text that pops up whenever the spawning fails for not having enough hunger.
    /// </summary>
    [DataField("popupText")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public string PopupText = "building-failure-hunger";

    /// <summary>
    /// What will be spawned at the end of the action.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public EntProtoId EntityProduced;

    /// <summary>
    /// The entity needed to actually preform the spawning. This will be granted (and removed) upon the entity's creation.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public EntProtoId Action = "ActionXenoResinWindow";

    [AutoNetworkedField]
    [DataField("actionEntity")]
    public EntityUid? ActionEntity;

    /// <summary>
    /// How long will it take to make.
    /// </summary>
    [DataField("productionLength")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float ProductionLength = 3f;

    /// <summary>
    /// This will subtract (not add, don't get this mixed up) from the current hunger of the mob doing the spawning.
    /// </summary>
    [DataField("hungerCost")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float HungerCost = 5f;

    /// <summary>
    /// The lowest hunger threshold that this mob can be in before it's allowed to spawn structures.
    /// </summary>
    [DataField("minHungerThreshold")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public HungerThreshold MinHungerThreshold = HungerThreshold.Okay;

    /// <summary>
    /// This gets played whenever the action is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundProgress;

    /// <summary>
    /// This gets played when the action is completed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundFinished;

    /// <summary>
    /// Audio entity used during the spawning in case the doafter gets canceled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SpawnStream;
}
