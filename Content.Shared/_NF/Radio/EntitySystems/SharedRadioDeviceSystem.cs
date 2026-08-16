using Content.Shared._NC.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.UserInterface;

namespace Content.Shared.Radio.EntitySystems;

public abstract partial class SharedRadioDeviceSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private const int MinRadioFrequency = 1;  // starcup: 1000 -> 1
    private const int MaxRadioFrequency = 3;  // starcup: 3000 -> 3


    [SubscribeLocalEvent]
    private void OnIntercomMapInit(EntityUid uid, IntercomComponent ent, MapInitEvent args)
    {
        // Set initial frequency (must be done regardless of power/enabled)
        if (ent.CurrentChannel != null &&
                ProtoMan.TryIndex(ent.CurrentChannel, out var channel) &&
                TryComp(uid, out RadioMicrophoneComponent? mic))
        {
            mic.Frequency = channel.Frequency;
        }
    }

    // Nuclear-14-Start
    #region Handheld Radio

    [SubscribeLocalEvent]
    private void OnBeforeHandheldRadioUiOpen(Entity<RadioMicrophoneComponent> microphone, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateHandheldRadioUi(microphone);
    }

    [SubscribeLocalEvent]
    private void OnToggleHandheldRadioMic(Entity<RadioMicrophoneComponent> microphone, ref ToggleHandheldRadioMicMessage args)
    {
        if (!args.Actor.Valid)
            return;

        SetMicrophoneEnabled(microphone.AsNullable(), args.Actor, args.Enabled, true);
        UpdateHandheldRadioUi(microphone);
    }

    [SubscribeLocalEvent]
    private void OnToggleHandheldRadioSpeaker(Entity<RadioMicrophoneComponent> microphone, ref ToggleHandheldRadioSpeakerMessage args)
    {
        if (!args.Actor.Valid)
            return;

        SetSpeakerEnabled(microphone.AsType(), args.Actor, args.Enabled, true);
        UpdateHandheldRadioUi(microphone);
    }

    [SubscribeLocalEvent]
    private void OnChangeHandheldRadioFrequency(Entity<RadioMicrophoneComponent> microphone, ref SelectHandheldRadioFrequencyMessage args)
    {
        if (!args.Actor.Valid)
            return;

        // Update frequency if valid and within range.
        if (args.Frequency >= MinRadioFrequency && args.Frequency <= MaxRadioFrequency)
            microphone.Comp.Frequency = args.Frequency;
        // Update UI with current frequency.
        UpdateHandheldRadioUi(microphone);
    }

    private void UpdateHandheldRadioUi(Entity<RadioMicrophoneComponent> radio)
    {
        var speakerComp = CompOrNull<RadioSpeakerComponent>(radio);
        var frequency = radio.Comp.Frequency;

        var micEnabled = radio.Comp.Enabled;
        var speakerEnabled = speakerComp?.Enabled ?? false;
        var state = new HandheldRadioBoundUIState(micEnabled, speakerEnabled, frequency);
        if (TryComp<UserInterfaceComponent>(radio, out var uiComp))
            _ui.SetUiState((radio.Owner, uiComp), HandheldRadioUiKey.Key, state); // Frontier: TrySetUiState<SetUiState
    }

    #endregion
    // Nuclear-14-End
}
