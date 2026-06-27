using Content.Shared._DV.CCVars;
using Content.Shared._starcup.Overlays;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client._starcup.Overlays;

public sealed partial class ScreenVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private ScreenVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreenVisionComponent, ComponentInit>(OnScreenVisionInit);
        SubscribeLocalEvent<ScreenVisionComponent, ComponentShutdown>(OnScreenVisionShutdown);
        SubscribeLocalEvent<ScreenVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ScreenVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnScreenVisionInit(EntityUid uid, ScreenVisionComponent component, ComponentInit args)
    {
        if (uid == _playerMan.LocalEntity && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnScreenVisionShutdown(EntityUid uid, ScreenVisionComponent component, ComponentShutdown args)
    {
        if (uid == _playerMan.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, ScreenVisionComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, ScreenVisionComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
