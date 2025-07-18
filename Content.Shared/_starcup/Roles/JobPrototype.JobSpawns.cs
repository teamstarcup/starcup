namespace Content.Shared.Roles;

public sealed partial class JobPrototype
{
    /// <summary>
    /// starcup: If a job should always spawn at its job spawn rather than late join spawn points or arrivals.
    /// Used for roles like prisoner that always need to spawn in specific locations.
    /// </summary>
    [DataField]
    public bool AlwaysUseJobSpawn { get; private set; }
}
