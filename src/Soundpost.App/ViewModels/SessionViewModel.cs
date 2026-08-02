using CommunityToolkit.Mvvm.ComponentModel;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>One app row in the per-app mixer, with a live volume + mute control.</summary>
public partial class SessionViewModel : ObservableObject
{
    private readonly IAudioSessionService _sessions;

    public int ProcessId { get; }

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _state = string.Empty;

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

    private bool _isMuted;

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (SetProperty(ref _isMuted, value))
            {
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

    public SessionViewModel(IAudioSessionService sessions, AudioSession session)
    {
        _sessions = sessions;
        ProcessId = session.ProcessId;
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
    }
}
