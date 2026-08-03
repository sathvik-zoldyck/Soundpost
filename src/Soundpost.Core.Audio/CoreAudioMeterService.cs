using NAudio.CoreAudioApi;

namespace Soundpost.Core.Audio;

/// <summary>
/// <see cref="IAudioMeterService"/> backed by the Core Audio peak meters (via NAudio).
/// Part of the COM firewall. Values are read fresh each poll; keep the poll rate modest
/// (20–30 Hz) since each call touches the default endpoint and its sessions.
/// </summary>
public sealed class CoreAudioMeterService : IAudioMeterService
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private bool _disposed;

    public float GetMasterPeak()
    {
        try
        {
            if (!_enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
            {
                return 0f;
            }

            using MMDevice device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device.AudioMeterInformation.MasterPeakValue;
        }
        catch
        {
            return 0f;
        }
    }

    public IReadOnlyDictionary<int, float> GetSessionPeaks()
    {
        var peaks = new Dictionary<int, float>();
        try
        {
            if (!_enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
            {
                return peaks;
            }

            using MMDevice device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            SessionCollection sessions = device.AudioSessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                try
                {
                    AudioSessionControl session = sessions[i];
                    peaks[(int)session.GetProcessID] = session.AudioMeterInformation.MasterPeakValue;
                }
                catch
                {
                    // Session closed mid-poll — skip.
                }
            }
        }
        catch
        {
            // No default device or transient COM failure.
        }

        return peaks;
    }

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
