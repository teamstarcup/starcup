using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class NonverbalLightAccentSystem : RelayAccentSystem<NonverbalLightAccentComponent>
{
    private static readonly Regex MessagePast8Characters = new(@"(?<=^.{8})(.+)");

    public override string Accentuate(string message, Entity<NonverbalLightAccentComponent>? ent = null)
    {

// Transforms everything past the 8th character to an ellipsis.
        message = MessagePast8Characters.Replace(message, "...");
            return message;
    }
}
