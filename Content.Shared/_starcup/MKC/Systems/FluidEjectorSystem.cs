using System.Linq;
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
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodstreamComponent, SolutionContainerChangedEvent>(OnSolutionChanged);
    }

    private void OnSolutionChanged(Entity<BloodstreamComponent> ent, ref SolutionContainerChangedEvent args)
    {

        // TODO: stop if handled

        // stop if you don't have a fluid ejector
        var ejectorList = _body.GetBodyOrganEntityComps<FluidEjectorComponent>((ent, null));
        if (ejectorList.Count == 0)
            return;

        // assemble all reagents in bloodstream
        var list = args.Solution.Contents.ToList();

        // TODO: check for presence of non-blood reagents in list

        // TODO: begin eject countdown

        // TODO: mark as handled

    }

    // TODO: DoEjectCountdown

        // TODO: define TimeSpan for countdown (datafield)

        // TODO: intermittent popups

        // TODO: once countdown is reached, DoFluidEject

    public void DoFluidEject(EntityUid uid)
    {
        var solution = new Solution();
        var solutionSize = 0f; // TODO: make solutionSize equal the volume of non-blood reagent in the bloodstream

        // TODO: transfer all non-blood reagents to our solution

        if (_puddle.TrySpillAt(uid, solution, out var puddle, false))
        {
            _forensics.TransferDna(puddle, uid, false);
        }

        // apply a bit of slowdown
        _movementMod.TryUpdateMovementSpeedModDuration(uid, MovementModStatusSystem.VomitingSlowdown, TimeSpan.FromSeconds(solutionSize), 0.5f);

        // TODO: apply drunkenness, scaling from solutionSize

        // TODO: popup

        // TODO: play sound effect

        // TODO: deal damage to entity, scaling from solutionSize

        // TODO: mark as no longer handled

        if (!_netManager.IsServer)
            return;
    }
}
