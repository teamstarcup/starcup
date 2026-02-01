using Content.Shared.Damage.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Shared._starcup.Chaplain;

public sealed class BlessedHealingSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SanctifiedComponent, BlessedHealingEvent>(OnHealAction);
    }

    private void OnHealAction(Entity<SanctifiedComponent> entity, ref BlessedHealingEvent ev)
    {
        if (!_mobStateSystem.IsAlive(ev.Target))
            return;

        ev.Handled = true;

        var uid = entity.Owner;
        var success = _damageableSystem.TryChangeDamage(ev.Target, entity.Comp.Damage, true, origin: uid);
        var othersMessageKey = success ? "blessed-heal-success-others" : "blessed-heal-success-none-others";
        var selfMessageKey = success ? "blessed-heal-success-self" : "blessed-heal-success-none-self";

        var othersMessage = Loc.GetString(othersMessageKey,
            ("user", Identity.Entity(ev.Performer, EntityManager)),
            ("target", Identity.Entity(ev.Target, EntityManager)),
            ("item", uid));
        _popupSystem.PopupEntity(othersMessage, ev.Performer, Filter.PvsExcept(ev.Performer), true, PopupType.Medium);

        var selfMessage = Loc.GetString(selfMessageKey,
            ("target", Identity.Entity(ev.Target, EntityManager)),
            ("item", uid));
        _popupSystem.PopupEntity(selfMessage, ev.Performer, ev.Performer, PopupType.Large);

        if (success)
            _audio.PlayPvs(entity.Comp.HealSound, ev.Performer);
    }
}
