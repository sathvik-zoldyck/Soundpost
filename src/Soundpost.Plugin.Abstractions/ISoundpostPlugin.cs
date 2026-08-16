using Soundpost.Core.Audio;

namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// The contract every plugin implements. A plugin is handed an <see cref="ISoundpostHost"/> on load and
/// then receives the events it cares about. Prefer deriving from <see cref="SoundpostPlugin"/>, which
/// gives no-op defaults so you only override what you need.
///
/// <para>Threading: the host invokes these on a dedicated plugin thread, never the audio COM thread or
/// the UI thread. Keep handlers quick and non-blocking; <see cref="OnAudioPeak"/> especially fires
/// often (~20 Hz per active session).</para>
/// </summary>
public interface ISoundpostPlugin
{
    // ---- identity ----

    /// <summary>The plugin's manifest.</summary>
    PluginInfo Info { get; }

    // ---- lifecycle ----

    /// <summary>Called once, before any event, with the host. Store it and do setup here.</summary>
    void OnLoaded(ISoundpostHost host);

    /// <summary>Called once when the plugin is being disabled or the app is shutting down. Undo/restore here.</summary>
    void OnUnloaded();

    // ---- device events ----

    /// <summary>An endpoint became available.</summary>
    void OnDeviceConnected(AudioDevice device);

    /// <summary>An endpoint went away.</summary>
    void OnDeviceDisconnected(AudioDevice device);

    /// <summary>The default endpoint changed for a role.</summary>
    void OnDefaultDeviceChanged(AudioDevice device, DeviceRole role);

    // ---- scene events ----

    /// <summary>A scene was applied.</summary>
    void OnSceneChanged(Scene scene);

    // ---- app (session) events ----

    /// <summary>A new app session appeared.</summary>
    void OnAppStarted(AudioSession session);

    /// <summary>An app session closed.</summary>
    void OnAppClosed(int processId);

    // ---- audio ----

    /// <summary>A throttled peak reading (~20 Hz) for one session, so plugins can react to loudness.</summary>
    void OnAudioPeak(AudioPeak peak);
}
