namespace Soundpost.Core.Audio;

/// <summary>
/// Live audio level metering. Peaks change many times per second, so — unlike the snapshot
/// models — this is polled directly by the UI's render timer rather than exposed as records.
/// </summary>
public interface IAudioMeterService : IDisposable
{
    /// <summary>Master peak (0.0–1.0) of the current default playback device.</summary>
    float GetMasterPeak();

    /// <summary>Per-process peak levels (0.0–1.0) for sessions on the default playback device.</summary>
    IReadOnlyDictionary<int, float> GetSessionPeaks();
}
