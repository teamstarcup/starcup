namespace Content.Shared._starcup.Anomaly;

/// <summary>
/// starcup: Attach to an entity for the anomaly locator to consider it a valid target.
/// </summary>
/// <remarks>
/// Created to work around rigidity with ProximityDetectionSystem: can't search for one component or another.
/// </remarks>
[RegisterComponent]
public sealed partial class AnomalyLocatorTargetComponent : Component;
