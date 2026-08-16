using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._starcup.Damage;

public sealed partial class CriticalDamageSystem : EntitySystem
{
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CriticalDamageComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CriticalDamageComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _gameTiming.CurTime + ent.Comp.UpdateInterval;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CriticalDamageComponent>();
        while (query.MoveNext(out var uid, out var critDamage))
        {
            if (_gameTiming.CurTime < critDamage.NextUpdate)
                continue;

            critDamage.NextUpdate += critDamage.UpdateInterval;

            if (_mobState.IsDead(uid))
                continue;

            if (_mobState.IsCritical(uid))
            {
                if (_gameTiming.CurTime >= critDamage.LastEmoteTime + critDamage.EmoteCooldown)
                {
                    critDamage.LastEmoteTime = _gameTiming.CurTime;
                    _chat.TryEmoteWithChat(uid,
                        critDamage.Emote,
                        ChatTransmitRange.HideChat,
                        ignoreActionBlocker: true);
                }

                TakeCriticalDamage((uid, critDamage));
                critDamage.CritDamageCycles += 1;
                continue;
            }

            StopCriticalDamage((uid, critDamage));
            critDamage.CritDamageCycles = 0;
        }
    }

    private void TakeCriticalDamage(Entity<CriticalDamageComponent> ent)
    {
        if (ent.Comp.CritDamageCycles >= 2)
            _adminLogger.Add(LogType.Asphyxiation, $"{ToPrettyString(ent):entity} started taking critical damage");

        _damageable.ChangeDamage(ent.Owner, ent.Comp.Damage, interruptsDoAfters: false, ignoreResistances: true);

        var ev = new CriticalDamageEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    private void StopCriticalDamage(Entity<CriticalDamageComponent> ent)
    {
        if (ent.Comp.CritDamageCycles >= 2)
            _adminLogger.Add(LogType.Asphyxiation, $"{ToPrettyString(ent):entity} stopped taking critical damage");

        var ev = new StopCriticalDamageEvent();
        RaiseLocalEvent(ent, ref ev);
    }
}

/// <summary>
/// Raised when an entity starts taking critical damage.
/// </summary>
[ByRefEvent]
public record struct CriticalDamageEvent;

/// <summary>
/// Raised when an entity stops taking critical damage.
/// </summary>
[ByRefEvent]
public record struct StopCriticalDamageEvent;
