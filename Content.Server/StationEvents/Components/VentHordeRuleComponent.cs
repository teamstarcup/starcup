using Content.Server.StationEvents.Events;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Audio; // starcup

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Component used for the vent horde gamerule.
/// Picks a random entity with <see cref="VentCritterSpawnLocationComponent"/>
/// and spawns entities picked from the <see cref="Table"/> on it after a delay.
/// </summary>
[RegisterComponent, Access(typeof(VentHordeRule))]
public sealed partial class VentHordeRuleComponent : Component
{
    /// <summary>
    /// The table of possible mobs to spawn from the vent.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>
    /// The vent that has been chosen to spawn the entities.
    /// Spawning logic is handled by <see cref="VentHordeSpawnerComponent"/>
    /// </summary>
    [DataField]
    public EntityUid? ChosenVent;

    /// <summary>
    /// starcup: Sound that will be used by <see cref="VentHordeSpawnerComponent"/> after the chosen vent is selected
    /// </summary>
    [DataField]
    public SoundSpecifier? PassiveSound;

    /// <summary>
    /// starcup: Sound that will play when entities are spawned by the <see cref="VentHordeSpawnerComponent"/>
    /// </summary>
    [DataField]
    public SoundSpecifier? EndSound;

    /// <summary>
    /// starcup: The chance that a vent will break when entities are spawned.
    /// </summary>
    [DataField]
    public float BreakChance;

    /// <summary>
    /// starcup: The entity that will replace the vent when it breaks.
    /// </summary>
    [DataField]
    public string? Replacement = "ScrapSteel";
}
