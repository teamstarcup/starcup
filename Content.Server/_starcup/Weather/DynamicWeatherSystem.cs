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
    private static readonly ProtoId<WeatherPrototype> WeatherClear = "Clear";

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

        ProtoId<WeatherPrototype>? initialStateProtoId = weatherScheduler.States.First().Key;
        if (dynamicWeather.RandomInitialState)
        {
            for (var i = 0; i < MaximumExpectedRoundLength / dynamicWeather.StepFrequency; i++)
            {
                initialStateProtoId = NextState(dynamicWeather, weatherScheduler);
                _proto.Resolve(initialStateProtoId, out dynamicWeather.CurrentState);
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

    private ProtoId<WeatherPrototype>? NextState(DynamicWeatherComponent dynamicWeather, WeatherSchedulerPrototype weatherScheduler)
    {
        var currentStateProto = dynamicWeather.CurrentState?.ID ?? WeatherClear;
        return SharedRandomExtensions.Pick(weatherScheduler.States[currentStateProto].Transitions, _robustRandom.GetRandom());
    }

    private void SetWeather(EntityUid map, DynamicWeatherComponent dynamicWeather, WeatherPrototype? weather)
    {
        if (weather != null && weather.ID == WeatherClear)
            weather = null;

        var previousState = dynamicWeather.CurrentState;
        dynamicWeather.CurrentState = weather;

        var mapId = Transform(map).MapID;
        _weather.SetWeather(mapId, weather, dynamicWeather.NextUpdate + WeatherComponent.ShutdownTime);

        if (!_proto.Resolve(dynamicWeather.Scheduler, out var weatherScheduler))
            return;

        if (previousState == weather)
            return;

        var ev = new DynamicWeatherUpdateEvent(map, previousState, weather);
        RaiseLocalEvent(map, ref ev, true);
    }

    private void SetWeather(EntityUid map, DynamicWeatherComponent dynamicWeather, ProtoId<WeatherPrototype>? weatherProtoId)
    {
        WeatherPrototype? weather = null;
        if (weatherProtoId != null && weatherProtoId != WeatherClear)
        {
            _proto.Resolve(weatherProtoId, out weather);
        }

        SetWeather(map, dynamicWeather, weather);
    }
}

/// <summary>
/// Raised when a map with dynamic weather switches from one weather state to another.
/// </summary>
[ByRefEvent]
public readonly record struct DynamicWeatherUpdateEvent(EntityUid DynamicWeather, WeatherPrototype? PreviousState, WeatherPrototype? NextState);
