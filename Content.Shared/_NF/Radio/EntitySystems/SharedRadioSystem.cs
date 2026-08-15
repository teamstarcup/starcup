using Content.Shared.FixedPoint;
using Content.Shared.Radio.Components;

namespace Content.Shared.Radio.EntitySystems;

public abstract partial class SharedRadioSystem : EntitySystem
{
    //Nuclear-14
    /// <summary>
    /// Gets the message frequency, if there is no such frequency, returns the standard channel frequency.
    /// </summary>
    public FixedPoint2 GetFrequency(EntityUid source, RadioChannelPrototype channel)
    {
        if (TryComp<RadioMicrophoneComponent>(source, out var radioMicrophone))
            return radioMicrophone.Frequency;

        return channel.Frequency;
    }
}
