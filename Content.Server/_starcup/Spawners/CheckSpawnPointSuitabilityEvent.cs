using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.Spawners;

/// <summary>
/// Raised when considering a spawn point for spawning a player. This is used primarily for determining if a late-join
/// or cryo bed spawn point is suitable for spawning a specific job (e.g. prisoners.) <see cref="SpawnPointSystem"/>,
/// <see cref="ContainerSpawnPointSystem"/> still check for the usual conditions.
/// </summary>
/// <param name="SpawnPointEntity"></param>
/// <param name="Job"></param>
/// <param name="HumanoidCharacterProfile"></param>
[ByRefEvent]
public record struct CheckSpawnPointSuitabilityEvent<T>(
    Entity<T> SpawnPointEntity,
    ProtoId<JobPrototype>? Job,
    HumanoidCharacterProfile? HumanoidCharacterProfile) where T : ISpawnPoint, IComponent
{
    /// <summary>
    /// Set this to true to skip considering this spawn point.
    /// </summary>
    public bool NotSuitable { get; set; }
}
