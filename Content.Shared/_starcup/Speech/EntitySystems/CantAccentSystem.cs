using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class CantAccentSystem : RelayAccentSystem<CantAccentComponent>
{
    private static readonly Regex RegexStartH = new(@"(?<!\w)h", RegexOptions.IgnoreCase);
    private static readonly Regex RegexLowerIng = new(@"ing\b");
    private static readonly Regex RegexUpperIng = new(@"ING\b");
    private static readonly Regex RegexLowerAnd = new(@"\band\b");
    private static readonly Regex RegexUpperAnd = new(@"\bAND\b");
    private static readonly Regex RegexLowerTtle = new(@"(tt)(?:le)");
    private static readonly Regex RegexUpperTtle = new(@"(TT)(?:LE)");

    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override string Accentuate(string message, Entity<CantAccentComponent>? ent = null)
    {
        // word replacements:
            var msg = _replacement.ApplyReplacements(message, "cant", ent?.Owner);

        // regex changes:
        // replaces h with ' at the start of words, as in happiness -> 'appiness (like the French accent)
        msg = RegexStartH.Replace(msg, "'");
        // replaces ing with in' at the end of words, as in feeling -> feelin' (like the Southern accent)
        msg = RegexLowerIng.Replace(msg, "in'");
        msg = RegexUpperIng.Replace(msg, "IN'");
        // shortens the word "and" to " an' " (like the Southern accent)
        msg = RegexLowerAnd.Replace(msg, "an'");
        msg = RegexUpperAnd.Replace(msg, "AN'");
        // replaces ttle with 'le, as in little -> li'le
        msg = RegexLowerTtle.Replace(msg, "'le");
        msg = RegexUpperTtle.Replace(msg, "'LE");

        return msg;
    }
}
