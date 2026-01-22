using Content.Shared._starcup.MKC;
using Robust.Client.Player;
using Robust.Shared.Timing;

namespace Content.Client._starcup.MKC;

public sealed class PowerCoreSystem : SharedPowerCoreSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly TimeSpan AlertUpdateDelay = TimeSpan.FromSeconds(0.5f);
    private TimeSpan _nextAlertUpdate = TimeSpan.Zero;
    private EntityQuery<PowerCoreComponent> _powerCoreQuery;

    public override void Initialize()
    {
        base.Initialize();

        _powerCoreQuery = GetEntityQuery<PowerCoreComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateBattery(frameTime);
    }

    public void UpdateBattery(float frameTime)
    {
        if (_player.LocalEntity is not {} localPlayer)
            return;

        if (_timing.CurTime < _nextAlertUpdate)
            return;

        _nextAlertUpdate = _timing.CurTime + AlertUpdateDelay;

        // _powerCoreQuery.TryComp()
        //
        // UpdateBatteryAlert((localPlayer, slot));

        // UpdateBatteryAlert()
    }
}
