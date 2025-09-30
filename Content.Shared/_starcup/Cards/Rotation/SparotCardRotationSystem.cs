using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Shared._starcup.Cards.Rotation;

public sealed class SparotCardRotationSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SparotCardRotationComponent, GetVerbsEvent<ActivationVerb>>(AddSpinVerb);
    }

    private void AddSpinVerb(EntityUid uid, SparotCardRotationComponent comp, GetVerbsEvent<ActivationVerb> args)
    {
        // if it doesn't have an actor, and we can't reach it, then don't add the verb
        if (!TryComp(args.User, out ActorComponent? actor))
            return;

        // this is to prevent ghosts from using it
        if (!args.CanInteract)
            return;

        var spinVerb = new ActivationVerb
        {
            Text = "Spin",
            // Icon = comp.VerbImage,
            Act = (() =>
            {
                if (!TryComp(uid, out TransformComponent? transform))
                    return;

                bool spun = _random.Next(1) == 1;

                if (spun)
                {
                    // get TransformComponent from entity.Owner
                    // ...
                    transform.LocalRotation += 180f;
                }


            }),
            Impact = LogImpact.Low,
        };

        spinVerb.Impact = LogImpact.Low;
        args.Verbs.Add(spinVerb);
    }
}
