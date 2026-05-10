using System.Linq;
using Content.Shared.Random.Helpers;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._starcup.Weather;

public sealed class DynamicWeatherSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    /// <summary>
    /// The meta-weather type that functions as a stand-in for no active weather event.
    /// </summary>
    private static readonly EntProtoId<WeatherStatusEffectComponent> WeatherClear = "WeatherClear";

    private static readonly TimeSpan MaximumExpectedRoundLength = TimeSpan.FromHours(6);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DynamicWeatherComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid entity, DynamicWeatherComponent dynamicWeather, MapInitEvent args)
    {
        if (!_proto.Resolve(dynamicWeather.Scheduler, out var weatherScheduler))
            return;

        EntProtoId<WeatherStatusEffectComponent>? initialStateProtoId = weatherScheduler.States.First().Key;
        if (dynamicWeather.RandomInitialState)
        {
            for (var i = 0; i < MaximumExpectedRoundLength / dynamicWeather.StepFrequency; i++)
            {
                initialStateProtoId = NextState(dynamicWeather, weatherScheduler);
                dynamicWeather.CurrentState = initialStateProtoId;
            }
        }

        if (initialStateProtoId == WeatherClear)
            initialStateProtoId = null;

        SetWeather(entity, dynamicWeather, initialStateProtoId);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<DynamicWeatherComponent, MapComponent>();
        while (query.MoveNext(out var entity, out var dynamicWeather, out var map))
        {
            if (now < dynamicWeather.NextUpdate)
                continue;

            dynamicWeather.NextUpdate = now + dynamicWeather.StepFrequency;

            if (!_proto.Resolve(dynamicWeather.Scheduler, out var weatherScheduler))
                continue;

            SetWeather(entity, dynamicWeather, NextState(dynamicWeather, weatherScheduler));
        }
    }

    private EntProtoId<WeatherStatusEffectComponent>? NextState(DynamicWeatherComponent dynamicWeather, WeatherSchedulerPrototype weatherScheduler)
    {
        var currentStateProto = dynamicWeather.CurrentState ?? WeatherClear;
        return _robustRandom.Pick(weatherScheduler.States[currentStateProto].Transitions);
    }

    private void SetWeather(EntityUid map, DynamicWeatherComponent dynamicWeather, EntProtoId<WeatherStatusEffectComponent>? weatherProto)
    {
        if (weatherProto != null && weatherProto.Equals(WeatherClear))
            weatherProto = null;

        var previousState = dynamicWeather.CurrentState;
        dynamicWeather.CurrentState = weatherProto;

        var mapId = Transform(map).MapID;
        _weather.TrySetWeather(mapId, weatherProto, out _, dynamicWeather.NextUpdate + SharedWeatherSystem.ShutdownTime);

        if (!_proto.Resolve(dynamicWeather.Scheduler, out var weatherScheduler))
            return;

        if (previousState == weatherProto)
            return;

        var ev = new DynamicWeatherUpdateEvent(map, previousState, weatherProto);
        RaiseLocalEvent(map, ref ev, true);
    }
}

/// <summary>
/// Raised when a map with dynamic weather switches from one weather state to another.
/// </summary>
[ByRefEvent]
public readonly record struct DynamicWeatherUpdateEvent(EntityUid DynamicWeather, EntProtoId<WeatherStatusEffectComponent>? PreviousState, EntProtoId<WeatherStatusEffectComponent>? NextState);
