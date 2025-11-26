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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DynamicWeatherComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid entity, DynamicWeatherComponent dynamicWeather, MapInitEvent args)
    {
        ProtoId<WeatherPrototype>? initialStateProtoId = dynamicWeather.States.First().Key;
        if (dynamicWeather.RandomInitialState)
        {
            var states = dynamicWeather.States.Keys.ToArray();
            initialStateProtoId = states[_robustRandom.Next(states.Length)];
        }

        if (initialStateProtoId == "Clear")
            initialStateProtoId = null;

        WeatherPrototype? initialState = null;
        if (initialStateProtoId != null && !_proto.Resolve(initialStateProtoId, out initialState))
            return;

        var mapId = Transform(entity).MapID;
        _weather.SetWeather(mapId, initialState, dynamicWeather.NextUpdate + WeatherComponent.ShutdownTime);
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

            var previousStateProtoId = dynamicWeather.CurrentState;
            NextState(dynamicWeather);

            WeatherPrototype? nextState = null;
            if (dynamicWeather.CurrentState != null && !_proto.Resolve(dynamicWeather.CurrentState, out nextState))
                return;

            WeatherPrototype? previousState = null;
            if (previousStateProtoId != null)
            {
                _proto.Resolve(previousStateProtoId, out previousState);
            }

            var ev = new DynamicWeatherUpdateEvent(entity, previousState, nextState);
            RaiseLocalEvent(entity, ref ev, true);

            _weather.SetWeather(map.MapId, nextState, dynamicWeather.NextUpdate + WeatherComponent.ShutdownTime);
        }
    }

    private void NextState(DynamicWeatherComponent dynamicWeather)
    {
        var previousState = dynamicWeather.CurrentState ?? dynamicWeather.States.First().Key;
        var pick = SharedRandomExtensions.Pick(dynamicWeather.States[previousState], _robustRandom.GetRandom());

        dynamicWeather.CurrentState = pick != "Clear" ? pick : (ProtoId<WeatherPrototype>?) null;
    }
}

/// <summary>
/// Raised when a map with dynamic weather switches from one weather state to another.
/// </summary>
[ByRefEvent]
public readonly record struct DynamicWeatherUpdateEvent(EntityUid DynamicWeather, WeatherPrototype? PreviousState, WeatherPrototype? NextState);
