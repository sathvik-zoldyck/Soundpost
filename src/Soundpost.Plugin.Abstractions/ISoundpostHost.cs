using Soundpost.Core.Audio;

namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// A plugin's single, sanctioned door into Soundpost: it <em>reads</em> current state and <em>acts</em>,
/// and never touches Windows audio COM directly — that stays behind the firewall. Every <c>act</c>
/// method is idempotent and logged. A plugin receives its host in
/// <see cref="ISoundpostPlugin.OnLoaded"/>.
/// </summary>
public interface ISoundpostHost
{
    // ---- read ----

    /// <summary>All endpoints of the given kind, as immutable snapshots.</summary>
    IReadOnlyList<AudioDevice> GetDevices(AudioDeviceKind kind);

    /// <summary>The current default endpoint for a kind + role, or <c>null</c> if there is none.</summary>
    AudioDevice? GetDefaultDevice(AudioDeviceKind kind, DeviceRole role = DeviceRole.Multimedia);

    /// <summary>The current per-app sessions (the mixer rows).</summary>
    IReadOnlyList<AudioSession> GetSessions();

    /// <summary>The saved scenes.</summary>
    IReadOnlyList<Scene> GetScenes();

    // ---- act (each idempotent and logged) ----

    /// <summary>Make a device the default for the given roles (all common roles if none are passed).</summary>
    void SetDefaultDevice(string deviceId, params DeviceRole[] roles);

    /// <summary>Route one app's audio to a specific endpoint.</summary>
    void RouteApp(int processId, string deviceId);

    /// <summary>Set an app's volume scalar, 0..1.</summary>
    void SetAppVolume(int processId, float level);

    /// <summary>Mute or unmute an app.</summary>
    void SetAppMute(int processId, bool mute);

    /// <summary>Apply a saved scene by id.</summary>
    void ApplyScene(string sceneId);

    // ---- utilities ----

    /// <summary>This plugin's private key/value store.</summary>
    IPluginStorage Storage { get; }

    /// <summary>The app log, tagged with this plugin's id.</summary>
    IPluginLog Log { get; }

    /// <summary>This plugin's private data directory (<c>%AppData%\Soundpost\plugins\&lt;id&gt;</c>).</summary>
    string DataDirectory { get; }
}
