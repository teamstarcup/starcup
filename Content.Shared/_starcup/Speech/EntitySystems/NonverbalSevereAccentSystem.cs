using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class NonverbalSevereAccentSystem : RelayAccentSystem<NonverbalSevereAccentComponent>
{
    private static readonly Regex UnwantedCharacters = new(@"([a-z]+|\.|,|'|;|:|\s)");
    private static readonly Regex UnwantedUpperCase = new(@"([A-Z]+)");

    public override string Accentuate(string message, Entity<NonverbalSevereAccentComponent>? ent = null)
    {

// Transforms all characters into periods except tone indicator punctuation like ?, !, and ~.
// There is a better way to do this. A better world is possible. It is beyond this one's skill to take us there.
        message = UnwantedCharacters.Replace(message, ".");
        message = UnwantedUpperCase.Replace(message, "..");
            return message;
    }
}
