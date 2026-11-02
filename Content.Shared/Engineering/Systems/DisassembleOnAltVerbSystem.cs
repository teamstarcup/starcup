using Content.Shared.DoAfter;
using Content.Shared.Engineering.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.Engineering.Systems;

public sealed partial class DisassembleOnAltVerbSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedHandsSystem _handsSystem = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!; // starcup: Add sounds
    [Dependency] private IGameTiming _timing = default!; // starcup: Add sounds

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DisassembleOnAltVerbComponent, GetVerbsEvent<AlternativeVerb>>(AddDisassembleVerb);
        SubscribeLocalEvent<DisassembleOnAltVerbComponent, DisassembleDoAfterEvent>(OnDisassembleDoAfter);
    }

    /// <summary>
    /// Begins disassembly of an entity.
    /// </summary>
    public void StartDisassembly(Entity<DisassembleOnAltVerbComponent> entity, EntityUid user)
    {
        // Doafter setup
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            entity.Comp.DisassembleTime,
            new DisassembleDoAfterEvent(),
            entity,
            entity)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void AddDisassembleVerb(Entity<DisassembleOnAltVerbComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || args.Hands == null)
            return;

        var user = args.User;

        // Actual verb stuff
        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                StartDisassembly(entity, user);
            },
            //Text = Loc.GetString("disassemble-system-verb-disassemble"), // starcup: customize verb text in component
            Text = Loc.GetString(entity.Comp.VerbText),
            Priority = 2

        };
        args.Verbs.Add(verb);
    }

    private void OnDisassembleDoAfter(Entity<DisassembleOnAltVerbComponent> entity, ref DisassembleDoAfterEvent args)
    {
        // begin starcup: original code below, unnecessary and prevents sound playing
        //if (!_net.IsServer || args.Cancelled) // This is odd but it works :)
        if (!_timing.IsFirstTimePredicted)
            return;
        // end starcup

        if (args.Cancelled)
            return;
        // begin starcup: play sound after doAfter completion
        _audio.PlayPredicted(entity.Comp.SoundFinished, entity, args.User, audioParams: null);
        // end starcup

        if (!_net.IsServer)
            return;

        if (TrySpawnNextTo(entity.Comp.PrototypeToSpawn, entity.Owner, out var spawnedEnt))
            _handsSystem.TryPickup(args.User, spawnedEnt.Value);

        QueueDel(entity.Owner);
    }
}
