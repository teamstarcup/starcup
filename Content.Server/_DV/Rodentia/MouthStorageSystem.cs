using Content.Shared._DV.Rodentia;
using Content.Shared.Speech;
using Content.Shared.Speech.EntitySystems;

namespace Content.Server._DV.Rodentia;

public sealed partial class MouthStorageSystem : SharedMouthStorageSystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MouthStorageComponent, AccentGetEvent>(OnAccent);
    }

    // Force you to mumble if you have items in your mouth
    private void OnAccent(EntityUid uid, MouthStorageComponent component, AccentGetEvent args)
    {
        if (IsMouthBlocked(component))
            args.Message = _replacement.ApplyReplacements(args.Message, "mumble");
    }
}
