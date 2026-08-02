using CommunityToolkit.Mvvm.ComponentModel;
using Fader.Core.Audio;

namespace Fader.App.ViewModels;

/// <summary>A playback device row in the device switcher.</summary>
public partial class DeviceViewModel : ObservableObject
{
    public string Id { get; }

    public AudioDeviceKind Kind { get; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isDefault;

    [ObservableProperty]
    private bool _isDefaultCommunications;

    [ObservableProperty]
    private string _state = string.Empty;

    public DeviceViewModel(AudioDevice device)
    {
        Id = device.Id;
        Kind = device.Kind;
        UpdateFrom(device);
    }

    public void UpdateFrom(AudioDevice device)
    {
        Name = device.Name;
        IsDefault = device.IsDefault;
        IsDefaultCommunications = device.IsDefaultCommunications;
        State = device.State.ToString();
    }
}
