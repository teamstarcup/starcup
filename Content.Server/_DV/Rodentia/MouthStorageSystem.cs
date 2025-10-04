using Content.Server.Speech.EntitySystems;  // starcup: fixed for upstream merge
using Content.Shared._DV.Rodentia;
using Content.Shared.Speech;

namespace Content.Server._DV.Rodentia;

public sealed class MouthStorageSystem : SharedMouthStorageSystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

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
