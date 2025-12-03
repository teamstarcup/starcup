using Content.Shared.Weather;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.Weather;

/// <summary>
/// Add this to a *map entity* to enable randomized weather.
/// </code>
///
/// Each state is a weather prototype ID, and lists the next possible weather states and their weighted chance.
/// </summary>
[RegisterComponent]
public sealed partial class DynamicWeatherComponent : Component
{
    [DataField(required: true)]
    public ProtoId<WeatherSchedulerPrototype> Scheduler;

    /// <summary>
    /// Wait this long before determining the next (random) weather state.
    /// </summary>
    /// <remarks>
    /// It's best to keep this above 15 seconds to match <see cref="Content.Shared.Weather.WeatherComponent.ShutdownTime"/>.
    /// </remarks>
    [DataField]
    public TimeSpan StepFrequency = TimeSpan.FromMinutes(1);

    /// <summary>
    /// When true, DynamicWeatherSystem will simulate weather transitions for <see cref="Content.Server._starcup.Weather.DynamicWeatherSystem.MaximumExpectedRoundLength"/> upon round start.
    /// </summary>
    [DataField]
    public bool RandomInitialState = true;

    [ViewVariables(VVAccess.ReadWrite)]
    public WeatherPrototype? CurrentState;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextUpdate;
}
