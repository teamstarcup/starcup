using Content.Shared._DV.QuickPhrase;
using Content.Shared._starcup.QuickPhrase; // starcup
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.AACTablet;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class AACTabletComponent : Component
{
    // Minimum time between each phrase, to prevent spam
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(1);

    // Time that the next phrase can be sent.
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextPhrase;

    /// <summary>
    /// starcup: Which groups of phrases the AAC tablet has access to.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<QuickPhraseGroupPrototype>> PhraseGroups;
}
