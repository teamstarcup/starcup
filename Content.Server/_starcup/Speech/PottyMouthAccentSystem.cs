using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech;

namespace Content.Server._starcup.Speech;

public sealed class PottyMouthAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

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
