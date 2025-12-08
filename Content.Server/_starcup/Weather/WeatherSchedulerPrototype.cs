using Content.Shared.Weather;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.Weather;

/// <summary>
/// Defines a preset for Markov chain-based weather scheduling. Describes how an environment tends towards weather
/// conditions.
/// </summary>
[Prototype]
public sealed class WeatherSchedulerPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public Dictionary<ProtoId<WeatherPrototype>, WeatherState> States = default!;
}

[Serializable, DataDefinition]
public partial struct WeatherState
{
    /// <summary>
    /// Describes possible weather states that may follow this one, and how likely they are to occur compared to others.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<WeatherPrototype>, float> Transitions = default!;
}
