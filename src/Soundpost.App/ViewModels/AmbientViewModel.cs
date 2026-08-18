using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>
/// The ambient soundscape mixer: a palette of looping sounds you can layer and balance to build a
/// backdrop for focus, sleep, or relaxation. Owns the sound list and the "stop all" action; the
/// actual playback is delegated to an <see cref="IAmbientPlayer"/>.
/// </summary>
public partial class AmbientViewModel : ObservableObject
{
    // id, display name, glyph key. The starter palette; more can be added (and eventually user-imported).
    private static readonly (string Id, string Name, string Icon)[] Catalog =
    {
        ("rain", "Rain", "Rain"),
        ("storm", "Storm", "Storm"),
        ("wind", "Wind", "Wind"),
        ("waves", "Waves", "Waves"),
        ("stream", "Stream", "Stream"),
        ("birds", "Birds", "Birds"),
        ("night", "Summer Night", "Night"),
        ("fire", "Fireplace", "Fire"),
        ("train", "Train", "Train"),
        ("coffee", "Coffee Shop", "Coffee"),
        ("city", "City", "City"),
        ("white-noise", "White Noise", "WhiteNoise"),
        ("pink-noise", "Pink Noise", "PinkNoise"),
        ("brown-noise", "Brown Noise", "BrownNoise"),
    };

    public ObservableCollection<AmbientSound> Sounds { get; } = new();

    /// <summary>How many layers are currently playing — drives the header count and "Stop all".</summary>
    public int ActiveCount => Sounds.Count(s => s.IsActive);

    public bool AnyActive => ActiveCount > 0;

    public AmbientViewModel(IAmbientPlayer player)
    {
        foreach ((string id, string name, string icon) in Catalog)
        {
            var sound = new AmbientSound(player, id, name, icon, volume: 0.5f);
            sound.PropertyChanged += OnSoundChanged;
            Sounds.Add(sound);
        }
    }

    private void OnSoundChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AmbientSound.IsActive))
        {
            OnPropertyChanged(nameof(ActiveCount));
            OnPropertyChanged(nameof(AnyActive));
        }
    }

    [RelayCommand]
    private void StopAll()
    {
        foreach (AmbientSound s in Sounds)
        {
            s.Stop();
        }
    }
}
