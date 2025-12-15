using Robust.Shared.Serialization;

namespace Content.Shared._starcup.Abilities.Rest;

[Serializable, NetSerializable]
public enum RestState : byte
{
    NotResting,
    Resting
}
