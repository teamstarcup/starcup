using Robust.Shared.Configuration;

namespace Content.Shared._starcup.CCVars;

[CVarDefs]
public sealed class SCCVars
{
    ///<summary>
    ///    Toggles whether the player sprints or walks by default.
    ///</summary>
    public static readonly CVarDef<bool> WalkByDefault = CVarDef.Create("control.walk_by_default", false, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);
}
