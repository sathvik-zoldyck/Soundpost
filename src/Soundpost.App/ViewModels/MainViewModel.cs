using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>
/// Backs the dashboard: the live list of playback devices (with one-click switching) and the
/// per-app mixer. Device changes arrive via events; sessions are polled on a timer (there is no
/// system-wide session-added event to subscribe to yet).
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IAudioDeviceService _devices;
    private readonly IAudioSessionService _sessions;
    private readonly IDefaultDeviceSwitcher _switcher;
    private readonly DispatcherTimer _timer;

    public ObservableCollection<DeviceViewModel> PlaybackDevices { get; } = new();

    public ObservableCollection<SessionViewModel> Sessions { get; } = new();

    [ObservableProperty]
    private string _defaultDeviceName = "—";

    public MainViewModel(
        IAudioDeviceService devices,
        IAudioSessionService sessions,
        IDefaultDeviceSwitcher switcher)
    {
        _devices = devices;
        _sessions = sessions;
        _switcher = switcher;

        _devices.DevicesChanged += OnDevicesChanged;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        _timer.Tick += (_, _) => RefreshSessions();
        _timer.Start();

        RefreshDevices();
        RefreshSessions();
    }

    [RelayCommand]
    private void SetDefault(DeviceViewModel? device)
    {
        if (device is null)
        {
            return;
        }

        try
        {
            _switcher.SetDefaultForAllRoles(device.Id);
        }
        catch
        {
            // Surfaced via Diagnostics in a later milestone; ignore for now.
        }

        RefreshDevices();
    }

    private void OnDevicesChanged(object? sender, AudioDeviceChange e) =>
        Application.Current?.Dispatcher.Invoke(RefreshDevices);

    private void RefreshDevices()
    {
        IReadOnlyList<AudioDevice> current = _devices.GetDevices(AudioDeviceKind.Playback);

        for (int i = PlaybackDevices.Count - 1; i >= 0; i--)
        {
            if (current.All(d => d.Id != PlaybackDevices[i].Id))
            {
                PlaybackDevices.RemoveAt(i);
            }
        }

        foreach (AudioDevice device in current)
        {
            DeviceViewModel? existing = PlaybackDevices.FirstOrDefault(x => x.Id == device.Id);
            if (existing is null)
            {
                PlaybackDevices.Add(new DeviceViewModel(device));
            }
            else
            {
                existing.UpdateFrom(device);
            }
        }

        DefaultDeviceName = _devices.GetDefaultDevice(AudioDeviceKind.Playback)?.Name ?? "—";
    }

    private void RefreshSessions()
    {
        IReadOnlyList<AudioSession> current = _sessions.GetSessions();

        for (int i = Sessions.Count - 1; i >= 0; i--)
        {
            if (current.All(s => s.ProcessId != Sessions[i].ProcessId))
            {
                Sessions.RemoveAt(i);
            }
        }

        foreach (AudioSession session in current)
        {
            SessionViewModel? existing = Sessions.FirstOrDefault(x => x.ProcessId == session.ProcessId);
            if (existing is null)
            {
                Sessions.Add(new SessionViewModel(_sessions, session));
            }
            else
            {
                existing.UpdateFrom(session);
            }
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _devices.DevicesChanged -= OnDevicesChanged;
    }
}
