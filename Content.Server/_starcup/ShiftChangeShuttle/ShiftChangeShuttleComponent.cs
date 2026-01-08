using System.ComponentModel.DataAnnotations;

namespace Content.Server._starcup.Shuttles;

[RegisterComponent, Access(typeof(ShiftChangeShuttleSystem))]
public sealed partial class ShiftChangeShuttleComponent : Component
{
    [DataField]
    public EntityUid DepotGrid;

    [DataField]
    public Queue<ShiftChangeShuttleStop> TravelQueue = new();

    [DataField]
    public ShiftChangeShuttleStop? CurrentStop;

    [DataField]
    public bool PreDeparture;

    [DataField]
    public TimeSpan NextStopTime = TimeSpan.Zero;
}

[DataDefinition]
public sealed partial record ShiftChangeShuttleStop
{
    /// <summary>
    /// The grid entity which this shuttle should travel to.
    /// </summary>
    [Required] public EntityUid Destination;

    /// <summary>
    /// The time the shuttle will take before departing. This is optional but good for queueing departures upon
    /// arrival time.
    /// </summary>
    public TimeSpan PreDepartureTime = TimeSpan.FromSeconds(0);

    /// <summary>
    /// The time the shuttle will take to enter FTL.
    /// </summary>
    /// <remarks>
    /// Primarily good for kicking the shuttle directly into FTL.
    /// </remarks>
    public TimeSpan StartupTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The time the shuttle will take once in FTL to arrive at the destination.
    /// </summary>
    public TimeSpan TravelTime = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The time the shuttle will spend at the destination before considering the next travel stop.
    /// </summary>
    public TimeSpan IdleTime = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The tag-specified airlock that the shuttle should prioritize docking to.
    /// <seealso cref="Content.Server.Shuttles.Systems.EmergencyShuttleSystem.ShuttleDockResultType"/>
    /// </summary>
    public string? DockingPriorityTag;

    /// <summary>
    /// When this is true, any living passengers aboard the shuttle will be kicked off once it depart after this.
    /// </summary>
    public bool DumpPassengersBeforeDeparture;
}
