using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.QuickPhrase;

/// <summary>
/// Imp. Added this to enable custom AAC vocabularies
/// </summary>
[Prototype]
public sealed partial class QuickPhraseGroupPrototype : IPrototype
{
    /// <summary>
    /// The "in code name" of the object. Must be unique.
    /// </summary>
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;
}
