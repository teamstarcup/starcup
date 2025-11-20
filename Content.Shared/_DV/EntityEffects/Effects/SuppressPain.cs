using Content.Shared._DV.Pain;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class SuppressPainEntityEffectSystem : EntityEffectSystem<MetaDataComponent, SuppressPain>
{
    [Dependency] private readonly SharedPainSystem _painSystem = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<SuppressPain> args)
    {
        var suppressionTime = args.Effect.SuppressionTime * args.Scale;

        _painSystem.TrySuppressPain(entity, suppressionTime);
    }
}

public sealed partial class SuppressPain : EntityEffectBase<SuppressPain>
{
    /// <summary>
    /// How long should the pain suppression last for each metabolism cycle
    /// </summary>
    [DataField]
    public float SuppressionTime = 30f;

    /// <summary>
    /// The strength level of the pain suppression
    /// </summary>
    [DataField]
    public PainSuppressionLevel SuppressionLevel = PainSuppressionLevel.Normal;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-pain-suppression",
            ("chance", Probability),
            ("level", SuppressionLevel.ToString().ToLowerInvariant()));
}
