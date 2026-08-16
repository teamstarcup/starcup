using Content.Server._starcup.Speech.Components;
using Content.Shared.Speech;
using Content.Shared.Speech.EntitySystems;

namespace Content.Server._starcup.Speech.EntitySystems;

public sealed partial class PottyMouthAccentSystem : EntitySystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PottyMouthComponent, AccentGetEvent>(OnAccentGet);
    }

    private string Accentuate(string message)
    {
        return _replacement.ApplyReplacements(message, "pottymouth");
    }

    private void OnAccentGet(EntityUid uid, PottyMouthComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
