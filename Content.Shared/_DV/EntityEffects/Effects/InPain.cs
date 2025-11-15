using Content.Shared._DV.Pain;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class InPainEntityEffectSystem : EntityEffectSystem<MetaDataComponent, InPain>
{
    [Dependency] private readonly SharedPainSystem _painSystem = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<InPain> args)
    {
        var painTime = args.Effect.PainTime * args.Scale;

        _painSystem.TryApplyPain(entity, painTime);
    }
}

public sealed partial class InPain : EntityEffectBase<InPain>
{
    /// <summary>
    /// How long should the pain suppression last for each metabolism cycle
    /// </summary>
    [DataField]
    public float PainTime = 5f;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addicted",
            ("chance", Probability));
}
