using Content.Server._DV.Weather;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew; // starcup
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Weather;
using Content.Shared.Whitelist;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Weather;

/// <summary>
/// Handles weather damage for exposed entities.
/// </summary>
public sealed partial class WeatherEffectsSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    // [Dependency] private readonly IPrototypeManager _proto = default!; // starcup: unused
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!; // starcup

    private EntityQuery<MapGridComponent> _gridQuery;

    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<WeatherDamageStatusEffectComponent, StatusEffectComponent>();
        var now = _timing.CurTime;
        while (query.MoveNext(out var effectEnt, out var weather, out var status))
        {
            if (!status.Applied)
                continue;

            if (status.AppliedTo is not {} map)
                continue;

            if (now < weather.NextUpdate)
                continue;

            weather.NextUpdate = now + weather.UpdateDelay;

            // FIXME: determine how weather startup and ending should be handled now
            // start and end do no damage
            // if (data.State != WeatherState.Running)
            //     continue;

            UpdateDamage(map, weather);
            UpdateEffects(map, weather); // starcup
        }
    }

    private void UpdateDamage(EntityUid map, WeatherDamageStatusEffectComponent weather)
    {
        if (weather.Damage is not {} damage)
            return;

        var query = EntityQueryEnumerator<MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var mob, out var xform))
        {
            // don't give dead bodies 10000 burn, that's not fun for anyone
            if (xform.MapUid != map || mob.CurrentState == MobState.Dead)
                continue;

            // if not in space, check for being indoors
            if (xform.GridUid is {} gridUid && _gridQuery.TryComp(gridUid, out var grid))
            {
                var tile = _map.GetTileRef((gridUid, grid), xform.Coordinates);
                if (!_weather.CanWeatherAffect((gridUid, grid), tile))
                    continue;
            }

            if (_whitelist.IsWhitelistFailOrNull(weather.DamageBlacklist, uid))
                _damageable.TryChangeDamage(uid, damage, interruptsDoAfters: false);
        }
    }

    // starcup - allows weather to inflict status effects
    private void UpdateEffects(EntityUid map, WeatherDamageStatusEffectComponent weather)
    {
        if (weather.StatusEffect is not {} statusEffectId)
            return;

        var query = EntityQueryEnumerator<MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var mob, out var xform))
        {
            if (xform.MapUid != map || mob.CurrentState == MobState.Dead)
                continue;

            // if not in space, check for being indoors
            if (xform.GridUid is {} gridUid && _gridQuery.TryComp(gridUid, out var grid))
            {
                var tile = _map.GetTileRef((gridUid, grid), xform.Coordinates);
                if (!_weather.CanWeatherAffect((gridUid, grid), tile))
                    continue;
            }

            if (weather.Refresh)
                _statusEffects.TryUpdateStatusEffectDuration(uid, statusEffectId, TimeSpan.FromSeconds(10));
            else
                _statusEffects.TryAddStatusEffectDuration(uid, statusEffectId, TimeSpan.FromSeconds(10));

        }
    }
}


