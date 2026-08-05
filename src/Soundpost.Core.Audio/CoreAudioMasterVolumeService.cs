using NAudio.CoreAudioApi;

namespace Soundpost.Core.Audio;

/// <summary>
/// <see cref="IMasterVolumeService"/> backed by IAudioEndpointVolume on the default render
/// endpoint (via NAudio). Part of the COM firewall — the endpoint handle is opened and released
/// per call so we never hold a live COM reference across a device change.
/// </summary>
public sealed class CoreAudioMasterVolumeService : IMasterVolumeService
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private bool _disposed;

    public float GetVolume() => Read(device => device.AudioEndpointVolume.MasterVolumeLevelScalar, 0f);

    public bool GetMute() => Read(device => device.AudioEndpointVolume.Mute, false);

    public void SetVolume(float level)
    {
        float clamped = Math.Clamp(level, 0f, 1f);
        Write(device => device.AudioEndpointVolume.MasterVolumeLevelScalar = clamped);
    }

    public void SetMute(bool mute) => Write(device => device.AudioEndpointVolume.Mute = mute);

    private T Read<T>(Func<MMDevice, T> read, T fallback)
    {
        try
        {
            using MMDevice? device = TryGetDefaultRender();
            return device is null ? fallback : read(device);
        }
        catch
        {
            return fallback;
        }
    }

    private void Write(Action<MMDevice> write)
    {
        try
        {
            using MMDevice? device = TryGetDefaultRender();
            if (device is not null)
            {
                write(device);
            }
        }
        catch
        {
            // Endpoint vanished mid-call (unplugged, default switched) — the next poll re-syncs.
        }
    }

    private MMDevice? TryGetDefaultRender() =>
        _enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
            ? _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
            : null;

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
