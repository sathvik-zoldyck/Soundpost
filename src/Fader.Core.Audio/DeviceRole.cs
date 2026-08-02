namespace Fader.Core.Audio;

/// <summary>
/// Windows endpoint role. <see cref="Console"/> and <see cref="Multimedia"/> are usually the
/// same device; <see cref="Communications"/> is what calling apps (Teams, Discord, Zoom) use.
/// </summary>
public enum DeviceRole
{
    /// <summary>System sounds and general console output.</summary>
    Console,

    /// <summary>Media playback (music, video, games).</summary>
    Multimedia,

    /// <summary>Voice communication (calls).</summary>
    Communications,
}
