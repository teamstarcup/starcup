using Content.Shared.Radio.Components;
using Content.Shared._DV.AACTablet;
using Content.Shared._starcup.AACTablet;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.AACTablet;

public sealed partial class AACTabletSystem
{
    private HashSet<ProtoId<RadioChannelPrototype>> GetAvailableChannels(EntityUid entity)
    {
        var channels = new HashSet<ProtoId<RadioChannelPrototype>>();

        // Get all the intrinsic radio channels (IPCs, implants)
        if (TryComp(entity, out ActiveRadioComponent? intrinsicRadio))
            channels.UnionWith(intrinsicRadio.Channels);

        // Get the user's headset channels, if any
        if (TryComp(entity, out WearingHeadsetComponent? headset)
            && TryComp(headset.Headset, out ActiveRadioComponent? headsetRadio))
            channels.UnionWith(headsetRadio.Channels);

        return channels;
    }

    private void OnBoundUIOpened(Entity<AACTabletComponent> ent, ref BoundUIOpenedEvent args)
    {
        var state = new AACTabletBuiState(GetAvailableChannels(args.Actor));
        _userInterface.SetUiState(args.Entity, AACTabletKey.Key, state);
    }
}
