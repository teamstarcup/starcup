using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(VentClogRule))]
public sealed partial class VentCritterSpawnLocationComponent : Component
{
    // begin Den changes: check if spawn locations allow spawns
    [DataField]
    public bool CanSpawn { get; set; } = true;
    // end Den changes
}
