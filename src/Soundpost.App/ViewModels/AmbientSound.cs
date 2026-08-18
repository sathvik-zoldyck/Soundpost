using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>
/// One ambient sound layer in the soundscape mixer — a looping clip (rain, fire, …) with its own
/// on/off state and volume. Toggling and volume changes are pushed straight to the
/// <see cref="IAmbientPlayer"/>.
/// </summary>
public partial class AmbientSound : ObservableObject
{
    private readonly IAmbientPlayer _player;

    /// <summary>Stable key: identifies the layer to the player and picks its glyph.</summary>
    public string Id { get; }

    public string Name { get; }

    /// <summary>Glyph selector, resolved to a <c>Geometry</c> resource by the view.</summary>
    public string IconKey { get; }

    [ObservableProperty]
    private bool _isActive;

    private float _volume;

    /// <summary>Mix level, 0–1. Applies live while the layer is playing.</summary>
    public float Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, value) && IsActive)
            {
                _player.SetVolume(Id, value);
            }
        }
    }

    public AmbientSound(IAmbientPlayer player, string id, string name, string iconKey, float volume)
    {
        _player = player;
        Id = id;
        Name = name;
        IconKey = iconKey;
        _volume = volume;
    }

    /// <summary>Play this layer, or stop it if it's already playing.</summary>
    [RelayCommand]
    private void Toggle()
    {
        IsActive = !IsActive;
        _player.SetActive(Id, IsActive);
        if (IsActive)
        {
            _player.SetVolume(Id, _volume);
        }
    }

    /// <summary>Stop the layer without flipping through <see cref="ToggleCommand"/> (used by "Stop all").</summary>
    public void Stop()
    {
        if (IsActive)
        {
            IsActive = false;
            _player.SetActive(Id, false);
        }
    }
}
