using Robust.Shared.Configuration;

namespace Content.Shared._starcup.CCVars;

public sealed partial class SCCVars
{
    ///<summary>
    ///    Provides the time it takes for the shift change shuttle to arrive at the station.
    ///</summary>
    public static readonly CVarDef<int> ShiftChangeShuttleTravelTime = CVarDef.Create("shift_change_shuttle.travel_time", 240, CVar.SERVER | CVar.REPLICATED);
}
