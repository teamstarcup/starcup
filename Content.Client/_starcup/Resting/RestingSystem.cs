using Content.Client.DamageState;
using Content.Shared._starcup.Resting;
using Content.Shared.Bed.Sleep;
using Content.Shared.Mobs;
using Robust.Client.GameObjects;

namespace Content.Client._starcup.Resting;

public sealed class RestingSystem : SharedRestingSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RestComponent, SleepStateChangedEvent>(OnSleepChanged);
    }

    private void OnSleepChanged(Entity<RestComponent> ent, ref SleepStateChangedEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (args.FellAsleep)
        {
            foreach (var (layer, state) in ent.Comp.SleepingLayers)
                _sprite.LayerSetData((ent.Owner, sprite), layer, state);
        }
        else
        {
            foreach (var (layer, state) in ent.Comp.AwakeLayers)
                _sprite.LayerSetData((ent.Owner, sprite), layer, state);
        }
    }
}
