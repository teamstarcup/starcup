using Content.Server._DV.Speech.Components;
using Content.Shared.Speech;
using Content.Shared.Speech.EntitySystems;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed partial class IrishAccentSystem : EntitySystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IrishAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    // converts left word when typed into the right word. For example typing you becomes ye.
    public string Accentuate(string message, IrishAccentComponent component)
    {
        var msg = message;

        msg = _replacement.ApplyReplacements(msg, "irish");

        return msg;
    }

    private void OnAccentGet(EntityUid uid, IrishAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
