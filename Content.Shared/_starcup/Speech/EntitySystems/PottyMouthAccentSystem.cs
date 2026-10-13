using Content.Shared._starcup.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Shared._starcup.Speech.EntitySystems;

public sealed partial class PottyMouthSystem : RelayAccentSystem<PottyMouthComponent>
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override string Accentuate(string message, Entity<PottyMouthComponent>? ent = null)
    {
        var msg =
            _replacement.ApplyReplacements(message, "pottymouth", ent?.Owner);
        return msg;
    }
}
