using Robust.Shared.Configuration;

namespace Content.Shared._starcup.CCVars;

public sealed partial class SCCVars
{
    /// <summary>
    ///     URL of the Discord webhook which will relay faxes received by SyndComm.
    /// </summary>
    public static readonly CVarDef<string> DiscordAdminFaxWebhook =
        CVarDef.Create("starcup.discord.admin_fax_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);
}
