using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     Show typing indicator icon when player typing text in chat box.
///     Added automatically when player poses entity.
/// </summary>
// [Access(typeof(SharedTypingIndicatorSystem))] Cosmatic Drift - Restricted access breaks synth trait because it rewrites the speech bubble over the default race indicator
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // Cosmatic Drift - AutoGenerateComponentState fixes a bug with synth trait
public sealed partial class TypingIndicatorComponent : Component
{
    /// <summary>
    ///     Prototype id that store all visual info about typing indicator.
    /// </summary>
    [DataField("proto"), AutoNetworkedField] // Cosmatic Drift - AutoNetworkedField fixes a bug in synth trait
    public ProtoId<TypingIndicatorPrototype> TypingIndicatorPrototype = "default";

    /// <summary>
    ///  DeltaV - Allow the indicator to be temporarily overriden
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<TypingIndicatorPrototype>? TypingIndicatorOverridePrototype;
}
