namespace Content.Server._starcup.GameRules;

[RegisterComponent]
public sealed partial class ReturnShiftChangeShuttleRuleComponent : Component
{
    [DataField]
    public TimeSpan After = TimeSpan.FromMinutes(3);
}
