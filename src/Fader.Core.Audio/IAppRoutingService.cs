namespace Fader.Core.Audio;

/// <summary>
/// Per-application audio routing — send one app's audio to a specific endpoint independent of the
/// system default (e.g. Spotify to speakers while Discord stays on the headset). Backed by the
/// undocumented per-app endpoint API, the same one behind Windows' "App volume and device
/// preferences".
/// </summary>
/// <remarks>
/// Windows persists the override per application and only applies it to audio streams the app
/// opens <em>after</em> the change. An app that already has an open stream may need to be nudged
/// or restarted to honor a new route — a Windows limitation Fader detects and surfaces rather
/// than hides.
/// </remarks>
public interface IAppRoutingService
{
    /// <summary>Routes a process's audio of the given kind to a specific endpoint.</summary>
    void RouteApp(int processId, string deviceId, AudioDeviceKind kind = AudioDeviceKind.Playback);

    /// <summary>Clears a process's override so it follows the system default again.</summary>
    void ResetApp(int processId, AudioDeviceKind kind = AudioDeviceKind.Playback);

    /// <summary>Returns the endpoint id a process is pinned to, or null if it follows the default.</summary>
    string? GetAppRoute(int processId, AudioDeviceKind kind = AudioDeviceKind.Playback);

    /// <summary>Clears every per-application endpoint override on the system.</summary>
    void ResetAll();
}
