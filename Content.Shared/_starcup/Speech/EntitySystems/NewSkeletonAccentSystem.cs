using System.Text.RegularExpressions;
using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class NewSkeletonAccentSystem : RelayAccentSystem<NewSkeletonAccentComponent>
{
    private static readonly Regex RegexLowerCk = new("ck");
    private static readonly Regex RegexUpperCk = new("CK");

    public override string Accentuate(string message, Entity<NewSkeletonAccentComponent>? ent = null)
    {
            message = RegexLowerCk.Replace(message, "ck-ck-ck");
            message = RegexUpperCk.Replace(message, "CK-CK-CK");

            return message; // starcup
    }
}
