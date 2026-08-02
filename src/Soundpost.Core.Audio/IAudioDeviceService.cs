namespace Soundpost.Core.Audio;

/// <summary>
/// Reads Windows audio endpoints and raises change notifications.
/// This is the single gateway to endpoint state for the rest of the application —
/// no layer above <c>Soundpost.Core.Audio</c> should talk to Core Audio directly.
/// </summary>
public interface IAudioDeviceService : IDisposable
{
    /// <summary>Returns the endpoints of the given kind.</summary>
    /// <param name="kind">Playback or recording.</param>
    /// <param name="includeUnavailable">
    /// When true, also returns disabled/unplugged/absent endpoints (useful for diagnostics
    /// and for scenes that reference a device that's currently disconnected).
    /// </param>
    IReadOnlyList<AudioDevice> GetDevices(AudioDeviceKind kind, bool includeUnavailable = false);

    /// <summary>Returns the current default endpoint for a kind + role, or null if none exists.</summary>
    AudioDevice? GetDefaultDevice(AudioDeviceKind kind, DeviceRole role = DeviceRole.Multimedia);

    /// <summary>
    /// Raised when any endpoint is added, removed, changes state, or the default changes.
    /// May fire on a background/system (COM) thread — marshal to your UI thread yourself.
    /// </summary>
    event EventHandler<AudioDeviceChange>? DevicesChanged;
}
