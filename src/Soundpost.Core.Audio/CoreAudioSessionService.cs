using NAudio.CoreAudioApi;
using NAudioSessionState = NAudio.CoreAudioApi.Interfaces.AudioSessionState;

namespace Soundpost.Core.Audio;

/// <summary>
/// <see cref="IAudioSessionService"/> backed by the Windows Audio Session API (via NAudio).
/// Part of the COM firewall — snapshots each session into an immutable <see cref="AudioSession"/>
/// while the device handle is still open, so callers never hold a live COM reference.
/// </summary>
public sealed class CoreAudioSessionService : IAudioSessionService
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private bool _disposed;

    public IReadOnlyList<AudioSession> GetSessions()
    {
        MMDevice? device = TryGetDefaultRender();
        if (device is null)
        {
            return Array.Empty<AudioSession>();
        }

        using (device)
        {
            return ReadSessions(device);
        }
    }

    public IReadOnlyList<AudioSession> GetSessions(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Array.Empty<AudioSession>();
        }

        MMDevice? device = TryGetDevice(deviceId);
        if (device is null)
        {
            return Array.Empty<AudioSession>();
        }

        using (device)
        {
            return ReadSessions(device);
        }
    }

    public void SetVolume(int processId, float level)
    {
        level = Math.Clamp(level, 0f, 1f);
        ForEachMatchingSession(processId, session => session.SimpleAudioVolume.Volume = level);
    }

    public void SetMute(int processId, bool mute) =>
        ForEachMatchingSession(processId, session => session.SimpleAudioVolume.Mute = mute);

    private static IReadOnlyList<AudioSession> ReadSessions(MMDevice device)
    {
        var result = new List<AudioSession>();

        SessionCollection sessions;
        try
        {
            sessions = device.AudioSessionManager.Sessions;
        }
        catch
        {
            return result;
        }

        for (int i = 0; i < sessions.Count; i++)
        {
            try
            {
                AudioSessionControl session = sessions[i];
                uint pid = session.GetProcessID;
                result.Add(new AudioSession
                {
                    ProcessId = (int)pid,
                    DisplayName = ResolveName(session, pid),
                    Volume = session.SimpleAudioVolume.Volume,
                    IsMuted = session.SimpleAudioVolume.Mute,
                    State = ToState(session.State),
                    IconPath = SafeIconPath(session),
                    SessionIdentifier = SafeSessionId(session),
                });
            }
            catch
            {
                // The owning app closed mid-enumeration — skip this session.
            }
        }

        return result;
    }

    private void ForEachMatchingSession(int processId, Action<AudioSessionControl> action)
    {
        MMDevice? device = TryGetDefaultRender();
        if (device is null)
        {
            return;
        }

        using (device)
        {
            SessionCollection sessions;
            try
            {
                sessions = device.AudioSessionManager.Sessions;
            }
            catch
            {
                return;
            }

            for (int i = 0; i < sessions.Count; i++)
            {
                try
                {
                    AudioSessionControl session = sessions[i];
                    if (session.GetProcessID == (uint)processId)
                    {
                        action(session);
                    }
                }
                catch
                {
                    // Ignore a transient failure on a single session.
                }
            }
        }
    }

    private MMDevice? TryGetDefaultRender()
    {
        try
        {
            return _enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                ? _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private MMDevice? TryGetDevice(string deviceId)
    {
        try
        {
            return _enumerator.GetDevice(deviceId);
        }
        catch
        {
            return null;
        }
    }

    private static string ResolveName(AudioSessionControl session, uint pid)
    {
        try
        {
            string display = session.DisplayName;
            // Some system sessions expose a resource path like "@%SystemRoot%\...,-800"; skip those.
            if (!string.IsNullOrWhiteSpace(display) && !display.StartsWith('@'))
            {
                return display;
            }
        }
        catch
        {
            // Fall through to process-name resolution.
        }

        if (pid == 0)
        {
            return "System sounds";
        }

        try
        {
            using System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById((int)pid);
            return Capitalise(process.ProcessName);
        }
        catch
        {
            return $"PID {pid}";
        }
    }

    // Process names come through lowercase ("chrome"); the mixer reads better with them capitalised.
    private static string Capitalise(string name) =>
        string.IsNullOrEmpty(name) || char.IsUpper(name[0])
            ? name
            : char.ToUpperInvariant(name[0]) + name[1..];

    private static string? SafeIconPath(AudioSessionControl session)
    {
        try
        {
            string icon = session.IconPath;
            return string.IsNullOrWhiteSpace(icon) ? null : icon;
        }
        catch
        {
            return null;
        }
    }

    private static string SafeSessionId(AudioSessionControl session)
    {
        try
        {
            return session.GetSessionIdentifier ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static AudioSessionState ToState(NAudioSessionState state) => state switch
    {
        NAudioSessionState.AudioSessionStateActive => AudioSessionState.Active,
        NAudioSessionState.AudioSessionStateExpired => AudioSessionState.Expired,
        _ => AudioSessionState.Inactive,
    };

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _enumerator.Dispose();
    }
}
