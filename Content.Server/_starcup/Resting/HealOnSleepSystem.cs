using Content.Shared._starcup.Resting;
using Content.Shared.Bed.Components;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Server._starcup.Resting;

public sealed class HealOnSleepSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SleepingComponent, HealOnSleepComponent>();

        while (query.MoveNext(out var uid, out _, out var healComponent))
        {
            if (_timing.CurTime < healComponent.NextHealTime)
                continue;

            healComponent.NextHealTime = _timing.CurTime + TimeSpan.FromSeconds(healComponent.HealTime);

            // No healing if you're dead, also don't stack with bed buckling as it already has a sleeping bonus
            if (_mobStateSystem.IsDead(uid) || HasComp<HealOnBuckleHealingComponent>(uid))
                continue;

            _damageableSystem.TryChangeDamage(uid, healComponent.Damage, true, origin: uid);
        }
    }
}
