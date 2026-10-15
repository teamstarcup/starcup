using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Random.Helpers;
using Content.Shared.Speech.EntitySystems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class NonverbalLightAccentSystem : RelayAccentSystem<NonverbalLightAccentComponent>
{
    private static readonly Regex Message123 = new("(\b(.{3})(.{2})(.)(.+))");
    private static readonly Regex MessageCharacter456 = new("(\b(?:.{3})(.{3})(?:.{2})(?:.)(?:.+))");
    private static readonly Regex MessageCharacter78 = new("(\b(?:.{3})(?:.{3})(.{2})(?:.{3})(?:.+))");
    private static readonly Regex MessageCharacter91011 = new("(\b(?:.{3})(?:.{3})(?:.{2})(.{3})(?:.+))");
    private static readonly Regex MessageEverythingElse = new("(\b(?:.{3})(?:.{3})(?:.{2})(?:.{3})(.+))");

    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override string Accentuate(string message, Entity<NonverbalLightAccentComponent>? ent = null)
    {

        var random = ent.HasValue
            ? SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent.Value))
            : _random;


// Transforms the 9th, 10th, and 11th characters to - and everything past that to an ellipsis. TODO: Make this randomized.
//        message = MessageCharacter123.Replace(message, "");
//        message = MessageCharacter456.Replace(message, "");
//        message = MessageCharacter78.Replace(message, "");
        message = MessageCharacter91011.Replace(message, "-");
        message = MessageEverythingElse.Replace(message, "...");
            return message; // starcup
    }
}
