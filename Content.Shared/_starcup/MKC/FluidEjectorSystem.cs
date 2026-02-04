using System.Linq;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage.Systems;
using Content.Shared.Drunk;
using Content.Shared.Fluids;
using Content.Shared.Forensics.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Metabolism;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._starcup.MKC;

public sealed class FluidEjectorSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedDrunkSystem _drunk = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, SolutionContainerChangedEvent>(OnSolutionChanged);
    }

    public override void Update(float deltaTime)
    {
        var query = EntityQueryEnumerator<FluidEjectorComponent, OrganComponent>();
        while (query.MoveNext(out _, out var fluidEjector, out var organ))
        {
            if (fluidEjector.NextUpdate == TimeSpan.Zero)
                continue;

            if (organ.Body is not {} body)
                continue;

            if (fluidEjector.NextUpdate != TimeSpan.Zero && _gameTiming.CurTime >= fluidEjector.LastPopupTime + fluidEjector.PopupCooldown)
            {
                fluidEjector.LastPopupTime = _gameTiming.CurTime;
                _popup.PopupEntity(Loc.GetString("fluid-regulator-warning"), body, body, PopupType.LargeCaution);
            }

            if (_gameTiming.CurTime >= fluidEjector.NextUpdate)
            {
                fluidEjector.NextUpdate = TimeSpan.Zero;
                DoFluidEject(body, fluidEjector);
            }
        }
    }

    private void OnSolutionChanged(Entity<BodyComponent> ent, ref SolutionContainerChangedEvent args)
    {
        if (_mobState.IsDead(ent.Owner))
            return;

        // stop if you don't have a fluid ejector
        _body.TryGetOrgansWithComponent<FluidEjectorComponent>((ent, null), out var ejectorList);
        if (ejectorList.Count <= 0)
            return;

        var ev = new MetabolismExclusionEvent();
        RaiseLocalEvent(ent.Owner, ref ev);

        // check for presence of non-blood reagents that cannot be metabolized
        _body.TryGetOrgansWithComponent<MetabolizerComponent>(ent.AsNullable(), out var metabolizerOrgans);
        if (!args.Solution.Contents
                .Where(reagent => !ev.Reagents.Contains(reagent.Reagent))
                .Any(reagent => ShouldExpelReagent(reagent.Reagent, metabolizerOrgans)))
            return;

        var ejector = ejectorList.First().Comp;
        if (ejector.NextUpdate != TimeSpan.Zero)
            return;

        ejector.NextUpdate = _gameTiming.CurTime + ejector.EjectionTime;
    }

    /// <summary>
    /// Determines if a given reagent should be expelled from the body
    /// </summary>
    /// <param name="reagentId"></param>
    /// <param name="metabolizerOrgans"></param>
    /// <returns>true if any of the metabolizer organs can metabolize any of the reagent's metabolism groups</returns>
    private bool ShouldExpelReagent(ReagentId reagentId, List<Entity<MetabolizerComponent>> metabolizerOrgans)
    {
        if (!_prototypeManager.TryIndex<ReagentPrototype>(reagentId.Prototype, out var proto))
            return true;

        var metabolizerWhitelists = metabolizerOrgans
            .SelectMany(metabolizer => metabolizer.Comp.ReagentWhitelist ?? [])
            .Distinct();

        return !metabolizerWhitelists.Contains(proto);
    }

    private Solution? GetEjectedReagents(EntityUid uid)
    {
        var ejectedSolution = new Solution();

        BloodstreamComponent? bloodstream = null;
        if (!Resolve(uid, ref bloodstream))
            return null;

        if (!_solutionContainer.ResolveSolution(uid,
                bloodstream.BloodSolutionName,
                ref bloodstream.BloodSolution,
                out var bloodSolution))
        {
            Log.Error("Failed to resolve bloodstream solution for entity {}", uid);
            return null;
        }

        // collect reagents which should not be metabolized in the blood stream (because they are the entity's blood)
        var ev = new MetabolismExclusionEvent();
        RaiseLocalEvent(uid, ref ev);

        _body.TryGetOrgansWithComponent<MetabolizerComponent>(uid, out var metabolizerOrgans);
        var ejectableReagents = bloodSolution.Contents
            .Where(r => !ev.Reagents.Contains(r.Reagent))
            .Where(r => ShouldExpelReagent(r.Reagent, metabolizerOrgans))
            .ToList()
            .ShallowClone();
        foreach (var reagent in ejectableReagents)
        {
            bloodSolution.RemoveReagent(reagent);
            ejectedSolution.AddReagent(reagent);
        }

        return ejectedSolution;
    }

    private void DoFluidEject(EntityUid uid, FluidEjectorComponent fluidEjector)
    {
        var ejectedSolution = GetEjectedReagents(uid);
        if (ejectedSolution == null)
            return;

        var ejectedAmount = ejectedSolution.Volume;

        if (_puddle.TrySpillAt(uid, ejectedSolution, out var puddle, true))
            _forensics.TransferDna(puddle, uid, false);

        var slowdownTime = TimeSpan.FromSeconds((ejectedAmount * 0.4f).Value);
        _movementMod.TryUpdateMovementSpeedModDuration(uid,
            MovementModStatusSystem.VomitingSlowdown,
            slowdownTime,
            0.5f);

        var drunkennessTime = TimeSpan.FromSeconds((ejectedAmount * 0.4f + 40).Value);
        _drunk.TryApplyDrunkenness(uid, drunkennessTime);

        _popup.PopupEntity(Loc.GetString("fluid-regulator-eject", ("person", Identity.Entity(uid, EntityManager))), uid, PopupType.Large);

        var damage = ejectedAmount * fluidEjector.EjectionDamage * fluidEjector.EjectionDamageMultiplier;
        _damageableSystem.TryChangeDamage(uid, damage, ignoreResistances: true);
    }
}
