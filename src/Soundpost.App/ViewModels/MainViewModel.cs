using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>
/// Backs the console: sidebar navigation, the master volume dial, the live device list (with
/// one-click switching), the per-app mixer, and the master + channel peak meters. Device changes
/// arrive via events; sessions and master volume are polled on a slow timer, meter levels on a fast one.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IAudioDeviceService _devices;
    private readonly IAudioSessionService _sessions;
    private readonly IDefaultDeviceSwitcher _switcher;
    private readonly IAudioMeterService _meters;
    private readonly IMasterVolumeService _master;
    private readonly DispatcherTimer _sessionTimer;
    private readonly DispatcherTimer _meterTimer;

    public ObservableCollection<DeviceViewModel> PlaybackDevices { get; } = new();

    public ObservableCollection<SessionViewModel> Sessions { get; } = new();

    [ObservableProperty]
    private string _defaultDeviceName = "—";

    [ObservableProperty]
    private double _masterLevel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboard), nameof(IsVisualizer), nameof(IsMixer), nameof(IsPlaceholder))]
    private Section _activeSection = Section.Dashboard;

    public bool IsDashboard => ActiveSection == Section.Dashboard;

    public bool IsVisualizer => ActiveSection == Section.Visualizer;

    public bool IsMixer => ActiveSection == Section.Mixer;

    /// <summary>True for the sections that are on the roadmap but not wired up yet.</summary>
    public bool IsPlaceholder => !IsDashboard && !IsVisualizer && !IsMixer;

    public MainViewModel(
        IAudioDeviceService devices,
        IAudioSessionService sessions,
        IDefaultDeviceSwitcher switcher,
        IAudioMeterService meters,
        IMasterVolumeService master)
    {
        _devices = devices;
        _sessions = sessions;
        _switcher = switcher;
        _meters = meters;
        _master = master;

        _devices.DevicesChanged += OnDevicesChanged;

        _sessionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        _sessionTimer.Tick += (_, _) =>
        {
            RefreshSessions();
            SyncMasterVolume();
        };
        _sessionTimer.Start();

        _meterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(45) };
        _meterTimer.Tick += (_, _) => TickMeters();
        _meterTimer.Start();

        RefreshDevices();
        RefreshSessions();
        SyncMasterVolume();
    }

    // ---- master volume ----

    private double _masterVolume;

    /// <summary>Default endpoint volume, 0–1. Writing it applies to Windows immediately.</summary>
    public double MasterVolume
    {
        get => _masterVolume;
        set
        {
            if (SetProperty(ref _masterVolume, value))
            {
                OnPropertyChanged(nameof(MasterVolumePercent));
                try
                {
                    _master.SetVolume((float)value);
                }
                catch
                {
                    // Endpoint changed mid-drag; the next poll re-syncs.
                }
            }
        }
    }

    public int MasterVolumePercent => (int)Math.Round(_masterVolume * 100);

    private bool _isMasterMuted;

    public bool IsMasterMuted
    {
        get => _isMasterMuted;
        set
        {
            if (SetProperty(ref _isMasterMuted, value))
            {
                try
                {
                    _master.SetMute(value);
                }
                catch
                {
                    // Ignore transient failures.
                }
            }
        }
    }

    [RelayCommand]
    private void ToggleMasterMute() => IsMasterMuted = !IsMasterMuted;

    /// <summary>Pull Windows' current value in, without echoing it straight back out.</summary>
    private void SyncMasterVolume()
    {
        if (SetProperty(ref _masterVolume, _master.GetVolume(), nameof(MasterVolume)))
        {
            OnPropertyChanged(nameof(MasterVolumePercent));
        }

        SetProperty(ref _isMasterMuted, _master.GetMute(), nameof(IsMasterMuted));
    }

    // ---- navigation ----

    [RelayCommand]
    private void Navigate(string? section)
    {
        if (Enum.TryParse(section, out Section target))
        {
            ActiveSection = target;
        }
    }

    // ---- devices & sessions ----

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
