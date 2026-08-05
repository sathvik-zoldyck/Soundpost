namespace Soundpost.Core.Audio;

/// <summary>
/// Reads and writes the master volume of the current default playback endpoint —
/// the same value the Windows volume flyout shows.
/// </summary>
public interface IMasterVolumeService : IDisposable
{
    /// <summary>Master volume scalar, 0.0–1.0. Returns 0 when there is no default endpoint.</summary>
    float GetVolume();

    /// <summary>Sets the master volume scalar (clamped to 0.0–1.0).</summary>
    void SetVolume(float level);

    /// <summary>Whether the default endpoint is muted.</summary>
    bool GetMute();

    /// <summary>Mutes or unmutes the default endpoint.</summary>
    void SetMute(bool mute);
}
