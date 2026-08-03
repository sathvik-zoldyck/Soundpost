# Plugin SDK

> **Status: proposed / in design (RFC).** This document is the *target* contract so contributors can
> shape it and start prototyping. It is not loadable in the app yet — track progress on the
> [Roadmap](ROADMAP.md) and help design it via an [RFC](docs/rfcs/).

Soundpost is built to be extended **without touching the core**. There are three kinds of extension:

| Kind | Interface | What it does |
|---|---|---|
| **Plugin** | `ISoundpostPlugin` | Reacts to audio/device/scene events and drives actions |
| **Visualizer** | `IVisualizerRenderer` | A render style for the "Sound, seen" view |
| **Theme** | resource dictionary | Reskins the console |

Everything is C# / .NET — you write a small class library, drop it in the `plugins/` folder, and
Soundpost discovers it.

---

## 1. The plugin contract

A plugin implements `ISoundpostPlugin`. It gets a `Host` on load — its single, sanctioned door into
Soundpost — and receives events it cares about.

```csharp
public interface ISoundpostPlugin
{
    // ---- identity ----
    PluginInfo Info { get; }

    // ---- lifecycle ----
    void OnLoaded(ISoundpostHost host);
    void OnUnloaded();

    // ---- device events ----
    void OnDeviceConnected(AudioDevice device);
    void OnDeviceDisconnected(AudioDevice device);
    void OnDefaultDeviceChanged(AudioDevice device, DeviceRole role);

    // ---- scene events ----
    void OnSceneChanged(Scene scene);

    // ---- app (session) events ----
    void OnAppStarted(AudioSession session);
    void OnAppClosed(int processId);

    // ---- audio ----
    // Throttled peak signal (~20 Hz), so plugins can react to loudness without a capture thread.
    void OnAudioPeak(AudioPeak peak);
}
```

`PluginInfo` is your manifest in code:

```csharp
public sealed record PluginInfo
{
    public required string Id { get; init; }          // reverse-DNS, e.g. "com.you.autoduck"
    public required string Name { get; init; }         // display name
    public required string Version { get; init; }      // SemVer
    public required string Author { get; init; }
    public string? Description { get; init; }
    public string MinAppVersion { get; init; } = "0.1.0";
}
```

A base class provides no-op defaults so you only override what you need:

```csharp
public abstract class SoundpostPlugin : ISoundpostPlugin
{
    public abstract PluginInfo Info { get; }
    protected ISoundpostHost Host { get; private set; } = null!;

    public virtual void OnLoaded(ISoundpostHost host) => Host = host;
    public virtual void OnUnloaded() { }
    public virtual void OnDeviceConnected(AudioDevice device) { }
    public virtual void OnDeviceDisconnected(AudioDevice device) { }
    public virtual void OnDefaultDeviceChanged(AudioDevice device, DeviceRole role) { }
    public virtual void OnSceneChanged(Scene scene) { }
    public virtual void OnAppStarted(AudioSession session) { }
    public virtual void OnAppClosed(int processId) { }
    public virtual void OnAudioPeak(AudioPeak peak) { }
}
```

## 2. The host API

The `Host` is how a plugin *reads* state and *acts*. Plugins never touch Windows audio COM directly —
that stays behind the firewall.

```csharp
public interface ISoundpostHost
{
    // read
    IReadOnlyList<AudioDevice> GetDevices(AudioDeviceKind kind);
    AudioDevice? GetDefaultDevice(AudioDeviceKind kind, DeviceRole role = DeviceRole.Multimedia);
    IReadOnlyList<AudioSession> GetSessions();
    IReadOnlyList<Scene> GetScenes();

    // act (each is an idempotent, logged Action)
    void SetDefaultDevice(string deviceId, params DeviceRole[] roles);
    void RouteApp(int processId, string deviceId);
    void SetAppVolume(int processId, float level);
    void SetAppMute(int processId, bool mute);
    void ApplyScene(string sceneId);

    // extend
    void RegisterVisualizer(IVisualizerRenderer renderer);

    // utilities
    IPluginStorage Storage { get; }   // small key/value store, JSON-backed, per-plugin
    IPluginLog Log { get; }           // writes to the app log with your plugin id
    string DataDirectory { get; }     // %AppData%\Soundpost\plugins\<your-id>
}
```

Event payloads reuse the same immutable models the app uses (`AudioDevice`, `AudioSession`,
`DeviceRole` from `Soundpost.Core.Audio`) plus:

```csharp
public sealed record AudioPeak(int ProcessId, float Level);   // Level 0..1; ProcessId 0 = master
public sealed record Scene(string Id, string Name);
```

## 3. Visualizer plugins

The simplest and most fun extension. A renderer draws one frame from smoothed audio data.

```csharp
public interface IVisualizerRenderer
{
    string Name { get; }                       // shown in the style picker
    void Render(DrawingContext dc, VizFrame frame, VizParams p);
}

public readonly ref struct VizFrame
{
    public ReadOnlySpan<float> Bands { get; }   // log-spaced FFT magnitudes, 0..1
    public ReadOnlySpan<float> Waveform { get; }// recent samples, -1..1
    public double Time { get; }                 // seconds, advances with the Speed knob
    public Size Size { get; }                   // draw area
    public IReadOnlyList<Color> Palette { get; }
}

public readonly record struct VizParams(
    double Sensitivity, double Smoothing, double Glow, double Speed);
```

Register it from a plugin's `OnLoaded`, or contribute it directly to the app's built-in set under
[`visualizers/`](visualizers/):

```csharp
public override void OnLoaded(ISoundpostHost host) =>
    host.RegisterVisualizer(new MyRippleRenderer());
```

## 4. A complete example

Auto-duck music when a call starts — mute Spotify while Discord is active on your comms device:

```csharp
public sealed class AutoDuckPlugin : SoundpostPlugin
{
    public override PluginInfo Info => new()
    {
        Id = "com.example.autoduck",
        Name = "Auto-Duck",
        Version = "1.0.0",
        Author = "you",
        Description = "Mutes music while a voice-chat app is playing.",
    };

    private int _spotifyPid;

    public override void OnAppStarted(AudioSession session)
    {
        if (session.DisplayName.Contains("Spotify", StringComparison.OrdinalIgnoreCase))
            _spotifyPid = session.ProcessId;
    }

    public override void OnAudioPeak(AudioPeak peak)
    {
        // If Discord is making sound, duck Spotify.
        var discord = Host.GetSessions()
            .FirstOrDefault(s => s.DisplayName.Contains("Discord", StringComparison.OrdinalIgnoreCase));
        if (discord is null || _spotifyPid == 0) return;

        bool onCall = peak.ProcessId == discord.ProcessId && peak.Level > 0.02f;
        Host.SetAppMute(_spotifyPid, onCall);
    }
}
```

## 5. Packaging & loading

- Build a `net9.0-windows` class library that references the `Soundpost.Plugin.Abstractions` package
  (the interfaces above — nothing else from core).
- Ship a `plugin.json` manifest next to the DLL (mirrors `PluginInfo`, used before the assembly loads).
- Drop the folder into `%AppData%\Soundpost\plugins\<your-id>\` (or the repo's [`plugins/`](plugins/)
  for built-ins). Soundpost discovers it via a collectible `AssemblyLoadContext` so plugins can be
  enabled/disabled/updated without a restart.

## 6. Stability & versioning

- The plugin API follows **SemVer**. Within a major version, we don't break the interfaces above.
- Anything marked *experimental* in code may change; it's called out in release notes.
- Deprecations keep working for at least one minor cycle with a warning.

## 7. Trust & safety

Plugins run **in-process with full trust** — they can do anything your account can. Until a sandbox
exists (a future RFC), treat plugins like any other software you install:

- Only run plugins you trust or have read the source of.
- Soundpost itself stays true to its principles — no telemetry, local-only — but a third-party plugin
  is third-party code. The app will clearly label unsigned/community plugins.

---

Want to help design this for real? Open an [RFC](docs/rfcs/) or join the discussion. The interfaces
above are a starting point, not gospel — early contributors get to shape them.
