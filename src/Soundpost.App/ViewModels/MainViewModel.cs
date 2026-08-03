using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>
/// Backs the console: the live device list (with one-click switching), the per-app mixer, and the
/// master + channel peak meters. Device changes arrive via events; sessions are polled on a slow
/// timer and meter levels on a fast one.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IAudioDeviceService _devices;
    private readonly IAudioSessionService _sessions;
    private readonly IDefaultDeviceSwitcher _switcher;
    private readonly IAudioMeterService _meters;
    private readonly DispatcherTimer _sessionTimer;
    private readonly DispatcherTimer _meterTimer;

    public ObservableCollection<DeviceViewModel> PlaybackDevices { get; } = new();

    public ObservableCollection<SessionViewModel> Sessions { get; } = new();

    [ObservableProperty]
    private string _defaultDeviceName = "—";

    [ObservableProperty]
    private double _masterLevel;

    [ObservableProperty]
    private bool _showVisualizer;

    public MainViewModel(
        IAudioDeviceService devices,
        IAudioSessionService sessions,
        IDefaultDeviceSwitcher switcher,
        IAudioMeterService meters)
    {
        _devices = devices;
        _sessions = sessions;
        _switcher = switcher;
        _meters = meters;

        _devices.DevicesChanged += OnDevicesChanged;

        _sessionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        _sessionTimer.Tick += (_, _) => RefreshSessions();
        _sessionTimer.Start();

        _meterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(45) };
        _meterTimer.Tick += (_, _) => TickMeters();
        _meterTimer.Start();

        RefreshDevices();
        RefreshSessions();
    }

    [RelayCommand]
    private void ShowMixer() => ShowVisualizer = false;

    [RelayCommand]
    private void ShowVisualizerView() => ShowVisualizer = true;

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

    private void TickMeters()
    {
        MasterLevel = Decay(MasterLevel, _meters.GetMasterPeak());

        IReadOnlyDictionary<int, float> peaks = _meters.GetSessionPeaks();
        foreach (SessionViewModel session in Sessions)
        {
            float peak = peaks.TryGetValue(session.ProcessId, out float p) ? p : 0f;
            session.MeterLevel = Decay(session.MeterLevel, peak);
        }
    }

    // Instant attack, smooth release — the classic meter ballistics.
    private static double Decay(double previous, double peak) =>
        peak >= previous ? peak : (previous * 0.80) + (peak * 0.20);

    public void Dispose()
    {
        _meterTimer.Stop();
        _sessionTimer.Stop();
        _devices.DevicesChanged -= OnDevicesChanged;
    }
}
