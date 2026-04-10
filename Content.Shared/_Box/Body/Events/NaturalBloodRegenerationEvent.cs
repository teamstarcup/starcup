using Content.Shared.FixedPoint;

namespace Content.Shared._Box.Body.Events;

/// <summary>
/// Raised on an entity before they naturally regenerate blood to modify the amount.
/// </summary>
/// <param name="BloodRefreshAmount">The amount of blood the entity will gain or lose.</param>
/// <param name="BloodReferenceFactor">The target volume of blood in the entities bloodstream, as a multiplier of their max blood capacity..</param>
[ByRefEvent]
public record struct NaturalBloodRegenerationEvent(FixedPoint2 BloodRefreshAmount, float BloodReferenceFactor);
