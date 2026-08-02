namespace Fader.Core.Audio;

/// <summary>
/// Enumerates and controls per-application audio sessions (the per-app mixer).
/// Sessions are read from a specific playback endpoint; the parameterless
/// <see cref="GetSessions()"/> uses the current default playback device.
/// </summary>
public interface IAudioSessionService : IDisposable
{
    /// <summary>Sessions on the current default playback device.</summary>
    IReadOnlyList<AudioSession> GetSessions();

    /// <summary>Sessions on a specific playback device by endpoint id.</summary>
    IReadOnlyList<AudioSession> GetSessions(string deviceId);

    /// <summary>Sets an application's session volume (0.0–1.0) on the default playback device.</summary>
    void SetVolume(int processId, float level);

    /// <summary>Mutes or unmutes an application on the default playback device.</summary>
    void SetMute(int processId, bool mute);
}
