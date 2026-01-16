namespace Content.Server._EE.Silicon.BlindHealing;

[RegisterComponent]
public sealed partial class BlindHealingComponent : Component
{
    [DataField]
    public int DoAfterDelay = 3;

    /// <summary>
    ///     A multiplier that will be applied to the above if an entity is repairing themselves.
    /// </summary>
    [DataField]
    public float SelfHealPenalty = 4f;

    /// <summary>
    ///     Whether or not an entity is allowed to repair itself.
    /// </summary>
    [DataField]
    public bool AllowSelfHeal = true;

    [DataField(required: true)]
    public List<string> DamageContainers;

    // begin starcup: item cost instead of misusing the price
    /// <summary>
    ///     For stackable items, how many are required in the stack to do the repair
    /// </summary>
    [DataField]
    public int StackCost = 2;
    // end starcup
}
