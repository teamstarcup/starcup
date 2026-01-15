using Content.Shared._starcup.MKC.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Fluids;
using Content.Shared.Forensics.Systems;
using Content.Shared.Movement.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.MKC.Systems;

public sealed class FluidEjectorSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodstreamComponent, SolutionChangedEvent>(OnSolutionChanged);
    }

    private void OnSolutionChanged(Entity<BloodstreamComponent> ent, ref SolutionChangedEvent args)
    {

        // Stop if you don't have a fluid ejector
        var ejectorList = _body.GetBodyOrganEntityComps<FluidEjectorComponent>((ent, null));
        if (ejectorList.Count == 0)
            return;

    }

    public void DoFluidEject(EntityUid uid)
    {
        var solution = new Solution();
        var solutionSize = (MathF.Abs(thirstAdded) + MathF.Abs(hungerAdded)) / 6;

        // Apply a bit of slowdown
        _movementMod.TryUpdateMovementSpeedModDuration(uid, MovementModStatusSystem.VomitingSlowdown, TimeSpan.FromSeconds(solutionSize), 0.5f);

        // Adds a tiny amount of the chem stream from earlier along with vomit
        if (TryComp<BloodstreamComponent>(uid, out var bloodStream))
        {
            var ejectedAmount = solutionSize;

            // Flushes small portion of the chemicals removed from the bloodstream stream
            if (_solutionContainer.ResolveSolution(uid, bloodStream.BloodSolutionName, ref bloodStream.BloodSolution))
            {
                var ejectedChemstreamAmount = _bloodstream.FlushChemicals((uid, bloodStream), ejectedAmount);

                if (ejectedChemstreamAmount != null)
                {
                    solution.AddSolution(ejectedChemstreamAmount, _proto);
                    ejectedAmount -= (float)ejectedChemstreamAmount.Volume;
                }
            }

            // Makes a vomit solution the size of 90% of the chemicals removed from the chemstream
            solution.AddReagent(new ReagentId(VomitPrototype, _bloodstream.GetEntityBloodData((uid, bloodStream))), ejectedAmount);
        }

        if (_puddle.TrySpillAt(uid, solution, out var puddle, false))
        {
            _forensics.TransferDna(puddle, uid, false);
        }


        if (!_netManager.IsServer)
            return;
    }
}
