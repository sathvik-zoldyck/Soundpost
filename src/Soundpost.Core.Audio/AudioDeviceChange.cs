namespace Soundpost.Core.Audio;

/// <summary>What kind of endpoint change Windows reported.</summary>
public enum AudioDeviceChangeKind
{
    Added,
    Removed,
    StateChanged,
    DefaultChanged,
    PropertyChanged,
}

/// <summary>
/// A normalized endpoint-change notification. This is the raw signal the automation
/// engine listens to (e.g. "headphones connected" → apply a scene).
/// </summary>
/// <param name="Kind">The type of change.</param>
/// <param name="DeviceId">The affected endpoint id (may be empty for some default-cleared events).</param>
/// <param name="DeviceKind">Playback/recording, when known (default-changed events).</param>
/// <param name="Role">The role whose default changed, when applicable.</param>
public sealed record AudioDeviceChange(
    AudioDeviceChangeKind Kind,
    string DeviceId,
    AudioDeviceKind? DeviceKind = null,
    DeviceRole? Role = null);
