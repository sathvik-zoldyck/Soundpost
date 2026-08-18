using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soundpost.App.Interop;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>One app row in the per-app mixer, with a live volume + mute control and a peak meter.</summary>
public partial class SessionViewModel : ObservableObject
{
    private readonly IAudioSessionService _sessions;
    private readonly IAppRoutingService _routing;

    public int ProcessId { get; }

    /// <summary>The output-routing options (Default + each endpoint); the active one is highlighted.</summary>
    public ObservableCollection<RouteChoice> RouteChoices { get; } = new();

    /// <summary>Whether the detail strip (dB, solo, session info) is open for this row.</summary>
    [ObservableProperty]
    private bool _isExpanded;

    /// <summary>True while this app is the soloed one (everything else muted). Driven by the mixer.</summary>
    [ObservableProperty]
    private bool _isSoloed;

    [RelayCommand]
    private void ToggleExpand() => IsExpanded = !IsExpanded;

    /// <summary>The label for the app's current output route — "Default" or the pinned endpoint name.</summary>
    public string RouteName
    {
        get
        {
            foreach (RouteChoice c in RouteChoices)
            {
                if (c.IsActive)
                {
                    return c.Name;
                }
            }

            return "Default";
        }
    }

    /// <summary>Send this app's audio to the chosen endpoint, or back to the system default.</summary>
    [RelayCommand]
    private void Route(RouteChoice? choice)
    {
        if (choice is null)
        {
            return;
        }

        try
        {
            if (choice.DeviceId is null)
            {
                _routing.ResetApp(ProcessId);
            }
            else
            {
                _routing.RouteApp(ProcessId, choice.DeviceId);
            }
        }
        catch
        {
            // The app may have closed, or the endpoint vanished mid-click; the next rebuild re-syncs.
        }

        foreach (RouteChoice c in RouteChoices)
        {
            c.IsActive = ReferenceEquals(c, choice);
        }

        OnPropertyChanged(nameof(RouteName));
    }

    /// <summary>
    /// Rebuild the routing options from the current endpoint list, marking the one the app is
    /// actually pinned to (read back from Windows). Called when the device list changes.
    /// </summary>
    public void RebuildRoutes(IReadOnlyList<DeviceViewModel> devices)
    {
        string? current = null;
        try
        {
            current = _routing.GetAppRoute(ProcessId);
        }
        catch
        {
            // No route readable (app gone / access denied); fall back to "Default".
        }

        RouteChoices.Clear();
        RouteChoices.Add(new RouteChoice(deviceId: null, "Default", "Default", isActive: current is null));
        foreach (DeviceViewModel d in devices)
        {
            bool active = current is not null && string.Equals(current, d.Id, System.StringComparison.OrdinalIgnoreCase);

            // Use the short role name ("Headphones") and the endpoint's inferred glyph so the pill
            // reads like the device cards elsewhere, not a wall of "(Adapter Name)" text.
            RouteChoices.Add(new RouteChoice(d.Id, d.Title, d.IconKind, active));
        }

        OnPropertyChanged(nameof(RouteName));
    }

    /// <summary>The app's real Windows icon, or null when we fall back to a letter tile.</summary>
    public ImageSource? Icon { get; }

    /// <summary>True when <see cref="Icon"/> is missing, so the letter tile takes over.</summary>
    public bool HasNoIcon => Icon is null;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _state = string.Empty;

    /// <summary>Live peak level (0–1) for the channel meter; updated by the render/meter loop.</summary>
    [ObservableProperty]
    private double _meterLevel;

    private float _volume;

    /// <summary>Volume scalar (0–1). Setting it applies to Windows immediately.</summary>
    public float Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, value))
            {
                OnPropertyChanged(nameof(VolumePercent));
                OnPropertyChanged(nameof(VolumeDb));
                try
                {
                    _sessions.SetVolume(ProcessId, value);
                }
                catch
                {
                    // The app may have closed between refresh and drag; ignore.
                }
            }
        }
    }

    public int VolumePercent => (int)System.Math.Round(_volume * 100);

    /// <summary>The volume as decibels below full scale — the reading a mixing desk shows.</summary>
    public string VolumeDb => _isMuted || _volume <= 0.0001f
        ? "−∞ dB"
        : (20 * System.Math.Log10(_volume)).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " dB";

    private bool _isMuted;

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (SetProperty(ref _isMuted, value))
            {
                OnPropertyChanged(nameof(VolumeDb));
                try
                {
                    _sessions.SetMute(ProcessId, value);
                }
                catch
                {
                    // Ignore transient failures.
                }
            }
        }
    }

    public SessionViewModel(IAudioSessionService sessions, IAppRoutingService routing, AudioSession session)
    {
        _sessions = sessions;
        _routing = routing;
        ProcessId = session.ProcessId;
        Icon = AppIconLoader.ForProcess(session.ProcessId);
        UpdateFrom(session);
    }

    /// <summary>Refresh display fields from a fresh snapshot without re-applying to Windows.</summary>
    public void UpdateFrom(AudioSession session)
    {
        DisplayName = session.DisplayName;
        State = session.State.ToString();

        // Set the backing fields directly so we don't echo the value back to the audio service.
        if (SetProperty(ref _volume, session.Volume, nameof(Volume)))
        {
            OnPropertyChanged(nameof(VolumePercent));
        }

        SetProperty(ref _isMuted, session.IsMuted, nameof(IsMuted));
        OnPropertyChanged(nameof(VolumeDb));
    }
}
