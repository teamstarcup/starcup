using Content.Shared.Body.Events;

namespace Content.Shared._starcup.Metabolism;

public sealed class MetabolizerWhitelistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetabolizerWhitelistComponent, MetabolismWhitelistEvent>(OnMetabolismWhitelistCheck);
    }

    private static void OnMetabolismWhitelistCheck(Entity<MetabolizerWhitelistComponent> ent, ref MetabolismWhitelistEvent args)
    {
        if (ent.Comp.ReagentWhitelist == null)
            return;

        foreach (var reagent in ent.Comp.ReagentWhitelist)
        {
            args.Reagents.Add(reagent);
        }
    }
}
