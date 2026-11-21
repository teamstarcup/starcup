using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class NonverbalLightAccentSystem : RelayAccentSystem<NonverbalLightAccentComponent>
{
//    private static readonly Regex MessagePast8Characters = new(@"(?<=^.{8})(.+)");
    private static readonly Regex MessagePastThirdWord = new(@"(?<=^(?:\S{1,20}\s)(?:\S{1,20}\s)(?:\S{1,20}))(\s.+)");


    public override string Accentuate(string message, Entity<NonverbalLightAccentComponent>? ent = null)
    {

// Transforms everything past the 8th character to an ellipsis.
        message = MessagePastThirdWord.Replace(message, "...");
            return message;
    }
}
