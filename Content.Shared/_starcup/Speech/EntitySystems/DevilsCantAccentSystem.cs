using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class DevilsCantAccentSystem : RelayAccentSystem<DevilsCantAccentComponent>
{
    private static readonly Regex RegexLowerIng = new(@"ing\b");
    private static readonly Regex RegexUpperIng = new(@"ING\b");

    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override string Accentuate(string message, Entity<DevilsCantAccentComponent>? ent = null)
    {
        // word replacements:
            var msg = _replacement.ApplyReplacements(message, "devilscant", ent?.Owner);

        // regex changes:
        // replaces ing with en at the end of words, as in feeling -> feelen
        msg = RegexLowerIng.Replace(msg, "en");
        msg = RegexUpperIng.Replace(msg, "EN");

        return msg;
    }
}

// # regex: give [] to you -> give it tha
// # regex: gave [] to you -> gave it tha
// # regex: teach [] to you -> teach it tha
// # regex: taught [] to you -> taught it tha
// # regex: [word of at least 6 letters](no er|se|ce)ly -> [word]wise/[word]erly
// # regex: [I'm|We're] [word].|!|? -> [word] us.|!|?
// regex: Are you [word]? -> [word] tha?
// # regex: You're [word].|!|? -> [word] tha.|!|?
// regex: [word]'re -> [word]'s
// regex: those [word]s -> thon [word]s
