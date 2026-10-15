using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class DevilsCantAccentSystem : RelayAccentSystem<DevilsCantAccentComponent>
{
    private static readonly Regex RegexLowerThoseXs = new(@"((those\s|Those\s)(?:\w{3,})(?:s|se)\b)");
    private static readonly Regex RegexUpperThoseXs = new(@"((THOSE\s)(?:\w{3,})(?:S|SE)\b)"); // enjoy the simplicity.
    private static readonly Regex RegexLowerRe = new(@"(?<=\w\w)(?<!you|You)('re)(?!\w)");
    private static readonly Regex RegexUpperRe = new(@"(?<=\w\w)(?<!YOU)('RE)(?!\w)");
    private static readonly Regex RegexLowerIng = new(@"(?<=\w\w\w)(ing|in')(?!\w)");
    private static readonly Regex RegexUpperIng = new(@"(?<=\w\w\w)(ING|IN')(?!\w)");
    private static readonly Regex RegexLowerFul = new(@"(?:[a-z]{5,})((ful)l?)(?:y?)\b");
    private static readonly Regex RegexUpperFul = new(@"(?:[a-z]{5,})((FUL)L?)(?:Y?)\b"); // it's all downhill from here
    private static readonly Regex RegexLowerVerbXToYou = new(@"(?:give\s|gave\s|teach\s|taught\s|explain\s|explained\s)(?:([a-z]+\s{0,1}){1,3})(\sto\syou\b)");
    private static readonly Regex RegexUpperVerbXToYou = new(@"(?:GIVE\s|GAVE\s|TEACH\s|TAUGHT\s|EXPLAIN\s|EXPLAINED\s)(?:([A-Z]+\s{0,1}){1,3})(\sTO\sYOU\b)");
    // the three-word ones aren't working
    private static readonly Regex RegexLowerYoureX = new(@"(?<Group1>(\bYou're\s|\byou're\s))(?<Group2a>([a-z]))(?<Group2b>(([a-z]+(?<!ed))))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexUpperYoureX = new(@"(?<Group1>(\bYOU'RE\s))(?<Group2a>([A-Z]))(?<Group2b>(([A-Z]+(?<!ED))))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexLowerYoureXxxed = new(@"(?<Group1>(\bYou're\s|\byou're\s))(?<Group2a>([a-z]))(?<Group2b>((([a-z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexUpperYoureXxxed = new(@"(?<Group1>(\bYOU'RE\s))(?<Group2a>([A-Z]))(?<Group2b>((([A-Z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexLowerImX = new(@"(?<Group1>(\bI'm\s|We're\s|\bi'm\s|we're\s))(?<Group2a>([a-z]))(?<Group2b>(([a-z]+(?<!ed))))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexUpperImX = new(@"(?<Group1>(\bI'M\s|WE'RE\s))(?<Group2a>([A-Z]))(?<Group2b>(([A-Z]+(?<!ed))))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexLowerImXxxed = new(@"(?<Group1>(\bI'm\s|We're\s|\bi'm\s|we're\s))(?<Group2a>([a-z]))(?<Group2b>((([a-z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexUpperImXxxed = new(@"(?<Group1>(\bI'M\s|WE'RE\s))(?<Group2a>([A-Z]))(?<Group2b>((([A-Z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexLowerThisIsXxx = new(@"(?<Group1>(\bThis is\s|\bthis is\s))(?<Group2a>([a-z]))(?<Group2b>((([a-z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexUpperThisIsXxx = new(@"(?<Group1>(\bTHIS IS\s))(?<Group2a>([A-Z]))(?<Group2b>((([A-Z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexLowerThisIsntXxx = new(@"(?<Group1>(\bThis isn't\s|\bthis isn't\s))(?<Group2>((([a-z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");
    private static readonly Regex RegexUpperThisIsntXxx = new(@"(?<Group1>(\bTHIS ISN'T\s))(?<Group2>((([A-Z]+){2,3})))(?<Group3>(\.|\!|~|!!|$))");

    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override string Accentuate(string message, Entity<DevilsCantAccentComponent>? ent = null)
    {

        // Reorders certain sentences. "We're leaving!" -> "Leaving us!"
        // With longer/-ed words, add a comma. "We're not leaving!" -> "Not leaving, us!" and "You're screwed." -> "Screwed, tha."
        // We're not doing caps variations for most of this.
        message = RegexLowerYoureX.Replace(message, m => m.Groups["Group2a"].Value.ToUpper() + m.Groups["Group2b"].Value + " tha" + m.Groups["Group3"].Value);
        message = RegexUpperYoureX.Replace(message, m => m.Groups["Group2a"].Value + m.Groups["Group2b"].Value + " THA" + m.Groups["Group3"].Value);
        message = RegexLowerYoureXxxed.Replace(message, m => m.Groups["Group2a"].Value.ToUpper() + m.Groups["Group2b"].Value + ", tha" + m.Groups["Group3"].Value);
        message = RegexUpperYoureXxxed.Replace(message, m => m.Groups["Group2a"].Value + m.Groups["Group2b"].Value + ", THA" + m.Groups["Group3"].Value);
        message = RegexLowerImX.Replace(message, m => m.Groups["Group2a"].Value.ToUpper() + m.Groups["Group2b"].Value + " us" + m.Groups["Group3"].Value);
        message = RegexUpperImX.Replace(message, m => m.Groups["Group2a"].Value + m.Groups["Group2b"].Value + " US" + m.Groups["Group3"].Value);
        message = RegexLowerImXxxed.Replace(message, m => m.Groups["Group2a"].Value.ToUpper() + m.Groups["Group2b"].Value + ", us" + m.Groups["Group3"].Value);
        message = RegexUpperImXxxed.Replace(message, m => m.Groups["Group2a"].Value + m.Groups["Group2b"].Value + ", US" + m.Groups["Group3"].Value);
        message = RegexLowerThisIsXxx.Replace(message, m => m.Groups["Group2a"].Value.ToUpper() + m.Groups["Group2b"].Value + ", this" + m.Groups["Group3"].Value);
        message = RegexUpperThisIsXxx.Replace(message, m => m.Groups["Group2a"].Value + m.Groups["Group2b"].Value + ", THIS" + m.Groups["Group3"].Value);
        message = RegexLowerThisIsntXxx.Replace(message, m => "Not" + m.Groups["Group2"].Value + ", this" + m.Groups["Group3"].Value);
        message = RegexUpperThisIsntXxx.Replace(message, m => "NOT" + m.Groups["Group2"].Value + ", THIS" + m.Groups["Group3"].Value);

        // if not otherwise invoked, uses of "'re" turn into "'s", as in "they're" -> "they's"
        message = RegexLowerRe.Replace(message, "'s");
        message = RegexUpperRe.Replace(message, "'S");
        // replaces ing with en at the end of words, as in feeling -> feelen
        message = RegexLowerIng.Replace(message, "en");
        message = RegexUpperIng.Replace(message, "EN");
        message = RegexLowerFul.Replace(message, "some");
        message = RegexUpperFul.Replace(message, "SOME");
        message = RegexLowerThoseXs.Replace(message, "thum ");
        message = RegexUpperThoseXs.Replace(message, "THUM ");
        message = RegexLowerVerbXToYou.Replace(message, "tha");
        message = RegexUpperVerbXToYou.Replace(message, "THA");

        // word replacements: (we put this after to avoid messing with the syntax crimes)
        var msg = _replacement.ApplyReplacements(message, "devilscant", ent?.Owner);

        return msg;
    }
}

// # regex: give [] to you -> give it tha
// # regex: gave [] to you -> gave it tha
// # regex: teach [] to you -> teach it tha
// # regex: taught [] to you -> taught it tha
// regex: [word of at least 5 letters]ful[no letter] -> [word]some

// # regex: [new line]You're [word].|! -> [word (no ed at end)] tha.|!
// # regex: [new line][I'm|We're|I|We] [word (no ed at end)].|! -> [word] us.|!
// # regex: [new line]This is [word (no ed at end)].|! -> [word] this.|!
// # regex: [new line]This isn't [word (no ed at end)].|! -> Not [word] this.|!

// # regex: You're [up to three].|! -> [words], tha.|!
// # regex: [I'm|We're|I|We] [up to three].|! -> [word], us.|!
// # regex: This is [up to three].|! -> [words], this.|!
// # regex: This isn't [up to three].|! -> Not [words], this.|!

// regex: [word]'re -> [word]'s
// regex: those [word]s -> thum
