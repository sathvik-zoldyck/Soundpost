using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace Fader.Core.Audio;

/// <summary>
/// <see cref="IAudioDeviceService"/> backed by the Windows Core Audio MMDevice API (via NAudio).
/// Part of the COM firewall: nothing above <c>Fader.Core.Audio</c> references NAudio or COM types.
/// </summary>
public sealed class CoreAudioDeviceService : IAudioDeviceService
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly NotificationClient _notifications;
    private bool _disposed;

    public CoreAudioDeviceService()
    {
        _notifications = new NotificationClient(RaiseChanged);
        _enumerator.RegisterEndpointNotificationCallback(_notifications);
    }

    public event EventHandler<AudioDeviceChange>? DevicesChanged;

    public IReadOnlyList<AudioDevice> GetDevices(AudioDeviceKind kind, bool includeUnavailable = false)
    {
        DataFlow flow = ToDataFlow(kind);
        DeviceState stateMask = includeUnavailable
            ? DeviceState.Active | DeviceState.Disabled | DeviceState.Unplugged | DeviceState.NotPresent
            : DeviceState.Active;

        string? defaultId = TryGetDefaultId(flow, Role.Multimedia);
        string? commsId = TryGetDefaultId(flow, Role.Communications);

        var result = new List<AudioDevice>();
        foreach (MMDevice device in _enumerator.EnumerateAudioEndPoints(flow, stateMask))
        {
            using (device)
            {
                result.Add(Snapshot(device, kind, defaultId, commsId));
            }
        }

        return result;
    }

    public AudioDevice? GetDefaultDevice(AudioDeviceKind kind, DeviceRole role = DeviceRole.Multimedia)
    {
        DataFlow flow = ToDataFlow(kind);
        Role naudioRole = ToRole(role);

        if (!_enumerator.HasDefaultAudioEndpoint(flow, naudioRole))
        {
            return null;
        }

        using MMDevice device = _enumerator.GetDefaultAudioEndpoint(flow, naudioRole);
        string? multimediaId = TryGetDefaultId(flow, Role.Multimedia);
        string? commsId = TryGetDefaultId(flow, Role.Communications);
        return Snapshot(device, kind, multimediaId, commsId);
    }

    private static AudioDevice Snapshot(MMDevice device, AudioDeviceKind kind, string? defaultId, string? commsId) =>
        new()
        {
            Id = device.ID,
            Name = SafeName(device),
            Kind = kind,
            State = ToState(device.State),
            IsDefault = device.ID == defaultId,
            IsDefaultCommunications = device.ID == commsId,
        };

    private string? TryGetDefaultId(DataFlow flow, Role role)
    {
        try
        {
            if (!_enumerator.HasDefaultAudioEndpoint(flow, role))
            {
                return null;
            }

            using MMDevice device = _enumerator.GetDefaultAudioEndpoint(flow, role);
            return device.ID;
        }
        catch
        {
            // No default configured, or the endpoint vanished mid-call during a device change.
            return null;
        }
    }

    private void RaiseChanged(AudioDeviceChange change) => DevicesChanged?.Invoke(this, change);

    private static string SafeName(MMDevice device)
    {
        try
        {
            return device.FriendlyName;
        }
        catch
        {
            // FriendlyName opens the property store, which can fail for a device that is
            // mid-transition (just unplugged) or has a misbehaving driver.
            return "(unknown device)";
        }
    }

    private static DataFlow ToDataFlow(AudioDeviceKind kind) =>
        kind == AudioDeviceKind.Recording ? DataFlow.Capture : DataFlow.Render;

    private static Role ToRole(DeviceRole role) => role switch
    {
        DeviceRole.Console => Role.Console,
        DeviceRole.Communications => Role.Communications,
        _ => Role.Multimedia,
    };

    private static AudioDeviceState ToState(DeviceState state) => state switch
    {
        DeviceState.Active => AudioDeviceState.Active,
        DeviceState.Disabled => AudioDeviceState.Disabled,
        DeviceState.NotPresent => AudioDeviceState.NotPresent,
        DeviceState.Unplugged => AudioDeviceState.Unplugged,
        _ => AudioDeviceState.Unknown,
    };

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            _enumerator.UnregisterEndpointNotificationCallback(_notifications);
        }
        catch
        {
            // Enumerator may already be torn down by the time we dispose.
        }

        _enumerator.Dispose();
    }

    /// <summary>
    /// Bridges the COM <see cref="IMMNotificationClient"/> callbacks into one normalized delegate.
    /// Windows invokes these on internal RPC threads; keep the work minimal and non-reentrant.
    /// </summary>
    private sealed class NotificationClient(Action<AudioDeviceChange> onChange) : IMMNotificationClient
    {
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) =>
            onChange(new AudioDeviceChange(AudioDeviceChangeKind.StateChanged, deviceId));

        public void OnDeviceAdded(string pwstrDeviceId) =>
            onChange(new AudioDeviceChange(AudioDeviceChangeKind.Added, pwstrDeviceId));

        public void OnDeviceRemoved(string deviceId) =>
            onChange(new AudioDeviceChange(AudioDeviceChangeKind.Removed, deviceId));

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) =>
            onChange(new AudioDeviceChange(
                AudioDeviceChangeKind.DefaultChanged,
                defaultDeviceId ?? string.Empty,
                flow == DataFlow.Capture ? AudioDeviceKind.Recording : AudioDeviceKind.Playback,
                role switch
                {
                    Role.Console => DeviceRole.Console,
                    Role.Communications => DeviceRole.Communications,
                    _ => DeviceRole.Multimedia,
                }));

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) =>
            onChange(new AudioDeviceChange(AudioDeviceChangeKind.PropertyChanged, pwstrDeviceId));
    }
}
