using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._starcup.AACTablet;

[Serializable, NetSerializable]
public sealed class AACTabletBuiState(HashSet<ProtoId<RadioChannelPrototype>> radioChannels) : BoundUserInterfaceState
{
    public HashSet<ProtoId<RadioChannelPrototype>> RadioChannels = radioChannels;
}
