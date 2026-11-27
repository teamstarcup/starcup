using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class NonverbalSevereAccentSystem : RelayAccentSystem<NonverbalSevereAccentComponent>
{
    private static readonly Regex UnwantedCharacters = new(@"([^\?!~A-Z\s]+)"); // target everything but ?, !, ~, space, and capitals
    private static readonly Regex UnwantedSpaces = new(@"((?<![!?~])\s+)"); // target spaces not following ?/!/~
    private static readonly Regex UnwantedLettersUpper = new(@"([A-Z])"); // target capitals
    public override string Accentuate(string message, Entity<NonverbalSevereAccentComponent>? ent = null)
    {

// Transforms all characters into periods except tone indicator punctuation like ?, !, and ~.
// There is a better way to do this. A better world is possible. It is beyond this one's skill to take us there.
        message = UnwantedCharacters.Replace(message, "."); // replace each string w/ one dot
        message = UnwantedSpaces.Replace(message, "."); // replace each space w/ one dot
        message = UnwantedLettersUpper.Replace(message, ".."); // replace each capital with two dots
            return message;
    }
}
