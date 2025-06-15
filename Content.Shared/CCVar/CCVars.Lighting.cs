using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

// starcup: #38276 early merge
public sealed partial class CCVars
{
    public static readonly CVarDef<bool> AmbientOcclusion =
        CVarDef.Create("light.ambient_occlusion", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
