using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server._starcup.Speech.EntitySystems;

public sealed class NewSkeletonAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerCk = new("ck");
    private static readonly Regex RegexUpperCk = new("CK");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SouthernAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, SouthernAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = RegexLowerCk.Replace(message, "ck-ck-ck");
        message = RegexUpperCk.Replace(message, "CK-CK-CK");
        args.Message = message;
    }
};
