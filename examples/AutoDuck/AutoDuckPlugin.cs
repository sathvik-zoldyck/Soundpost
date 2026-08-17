using Soundpost.Core.Audio;
using Soundpost.Plugin.Abstractions;

namespace Soundpost.Examples.AutoDuck;

/// <summary>
/// Lowers every other app while a voice-chat app is actually making sound, then restores the exact
/// original volumes — the "duck the music when a call starts" reflex, automated.
///
/// <para>How it decides: a set of comms apps is matched by name (Discord, Teams, Zoom, …). Whenever one
/// of them peaks above a small activation level, we consider a call "live" and duck; we hold that for a
/// short release window so brief gaps between words don't pump the volume up and down. When the window
/// lapses, we put every ducked app back exactly where it was.</para>
///
/// <para>Everything is configurable through the plugin's own key/value storage (written with defaults on
/// first run) — see README.md. State is intentionally simple: the host calls plugins on a single
/// thread, so no locking is needed.</para>
/// </summary>
public sealed class AutoDuckPlugin : SoundpostPlugin
{
    public override PluginInfo Info => new()
    {
        Id = "com.soundpost.examples.autoduck",
        Name = "Auto-Duck",
        Version = "1.0.0",
        Author = "Soundpost contributors",
        Description = "Lowers other apps while a voice-chat app is playing, then restores them.",
    };

    // ---- configuration (loaded from storage, with defaults) ----
    private string[] _commsTokens = DefaultCommsTokens;
    private float _duckLevel = 0.25f;
    private int _releaseMs = 900;
    private float _activationLevel = 0.02f;

    // ---- state ----
    private readonly Dictionary<int, string> _names = new();       // pid -> display name
    private readonly HashSet<int> _commsPids = new();              // sessions that are comms apps
    private readonly Dictionary<int, float> _duckedOriginals = new(); // pid -> volume before ducking
    private bool _ducked;
    private long _lastCommsActiveMs = long.MinValue;

    private static readonly string[] DefaultCommsTokens =
        { "Discord", "Teams", "Zoom", "Slack", "Webex", "Meet", "Skype", "Mumble" };

    public override void OnLoaded(ISoundpostHost host)
    {
        base.OnLoaded(host);
        LoadConfig();

        // Seed from whatever is already running, so a call in progress when we load is handled.
        foreach (AudioSession session in Host.GetSessions())
        {
            Track(session);
        }

        Host.Log.Info(
            $"Auto-Duck ready — comms: [{string.Join(", ", _commsTokens)}], " +
            $"duck to {_duckLevel:P0}, release {_releaseMs} ms.");
    }

    public override void OnAppStarted(AudioSession session) => Track(session);

    public override void OnAppClosed(int processId)
    {
        _names.Remove(processId);
        _commsPids.Remove(processId);
        _duckedOriginals.Remove(processId); // the process is gone; nothing to restore
    }

    public override void OnAudioPeak(AudioPeak peak)
    {
        if (peak.ProcessId != 0 && peak.Level > _activationLevel && _commsPids.Contains(peak.ProcessId))
        {
            _lastCommsActiveMs = Environment.TickCount64;
        }

        bool callLive = _lastCommsActiveMs != long.MinValue
            && Environment.TickCount64 - _lastCommsActiveMs <= _releaseMs;

        if (callLive && !_ducked)
        {
            Duck();
        }
        else if (!callLive && _ducked)
        {
            Restore();
        }
    }

    public override void OnUnloaded()
    {
        // Leave nothing quiet behind us if we're disabled mid-call.
        if (_ducked)
        {
            Restore();
        }
    }

    private void Track(AudioSession session)
    {
        _names[session.ProcessId] = session.DisplayName;
        if (IsComms(session.DisplayName))
        {
            _commsPids.Add(session.ProcessId);
        }
    }

    private void Duck()
    {
        _ducked = true;
        int count = 0;

        foreach (AudioSession session in Host.GetSessions())
        {
            int pid = session.ProcessId;
            if (pid == 0 || _commsPids.Contains(pid) || _duckedOriginals.ContainsKey(pid))
            {
                continue; // never duck system sounds or the call itself, and never double-duck
            }

            _duckedOriginals[pid] = session.Volume;
            float target = Math.Min(session.Volume, _duckLevel); // only ever lower, never raise
            Host.SetAppVolume(pid, target);
            count++;
        }

        Host.Log.Info($"Call live — ducked {count} app(s) to {_duckLevel:P0}.");
    }

    private void Restore()
    {
        _ducked = false;
        foreach (KeyValuePair<int, float> original in _duckedOriginals)
        {
            Host.SetAppVolume(original.Key, original.Value);
        }

        int count = _duckedOriginals.Count;
        _duckedOriginals.Clear();
        Host.Log.Info($"Call ended — restored {count} app(s).");
    }

    private bool IsComms(string displayName)
    {
        foreach (string token in _commsTokens)
        {
            if (displayName.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // Reads config from the plugin's own storage, seeding the defaults on first run so a user has
    // something to edit. Values are clamped to sane ranges.
    private void LoadConfig()
    {
        IPluginStorage store = Host.Storage;

        string comms = store.Get("comms.apps") ?? string.Join(",", DefaultCommsTokens);
        if (store.Get("comms.apps") is null)
        {
            store.Set("comms.apps", comms);
        }

        _commsTokens = SplitTokens(comms);
        if (_commsTokens.Length == 0)
        {
            _commsTokens = DefaultCommsTokens;
        }

        _duckLevel = (float)Math.Clamp(store.GetDouble("duck.level", 0.25), 0.0, 1.0);
        _releaseMs = Math.Clamp(store.GetInt("duck.releaseMs", 900), 0, 10_000);
        _activationLevel = (float)Math.Clamp(store.GetDouble("comms.activationLevel", 0.02), 0.0, 1.0);

        if (store.Get("duck.level") is null)
        {
            store.Set("duck.level", "0.25");
        }

        if (store.Get("duck.releaseMs") is null)
        {
            store.Set("duck.releaseMs", "900");
        }

        if (store.Get("comms.activationLevel") is null)
        {
            store.Set("comms.activationLevel", "0.02");
        }
    }

    private static string[] SplitTokens(string csv)
    {
        string[] parts = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts;
    }
}
