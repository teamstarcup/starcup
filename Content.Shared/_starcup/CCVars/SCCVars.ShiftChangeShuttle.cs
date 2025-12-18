using Robust.Shared.Configuration;

namespace Content.Shared._starcup.CCVars;

public sealed partial class SCCVars
{
    ///<summary>
    ///    Toggles whether the player sprints or walks by default.
    ///</summary>
    public static readonly CVarDef<int> ShiftChangeShuttleTravelTime = CVarDef.Create("shift_change_shuttle.travel_time", 60, CVar.SERVER | CVar.REPLICATED);
}
