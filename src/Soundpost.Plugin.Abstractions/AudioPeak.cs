namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// A throttled loudness reading for one session (~20 Hz), so a plugin can react to audio without
/// running its own capture thread. <see cref="ProcessId"/> 0 is the master meter.
/// </summary>
/// <param name="ProcessId">The session's owning process id; 0 = master.</param>
/// <param name="Level">Peak level in 0..1.</param>
public readonly record struct AudioPeak(int ProcessId, float Level);
