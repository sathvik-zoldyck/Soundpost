using CommunityToolkit.Mvvm.ComponentModel;
using Soundpost.Core.Audio;

namespace Soundpost.App.ViewModels;

/// <summary>A playback device row in the device switcher.</summary>
public partial class DeviceViewModel : ObservableObject
{
    public string Id { get; }

    public AudioDeviceKind Kind { get; }

    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>Endpoint role, e.g. "Headphones" from "Headphones (HyperX Cloud II)".</summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>Hardware name, e.g. "HyperX Cloud II". Falls back to the endpoint state.</summary>
    [ObservableProperty]
    private string _subtitle = string.Empty;

    /// <summary>Which glyph to draw: Headphones, Speakers, Monitor or Generic.</summary>
    [ObservableProperty]
    private string _iconKind = "Generic";

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

        (Title, Subtitle) = SplitName(device.Name, device.State);
        IconKind = InferIcon(device.Name);
    }

    // Windows names endpoints "Role (Hardware)" — e.g. "Speakers (Realtek(R) Audio)". Splitting on the
    // last bracket gives us a headline plus the hardware underneath; anything else stays as-is.
    private static (string Title, string Subtitle) SplitName(string name, AudioDeviceState state)
    {
        string trimmed = name.Trim();
        int open = trimmed.LastIndexOf('(');
        if (open > 0 && trimmed.EndsWith(')'))
        {
            string title = trimmed[..open].Trim();
            string subtitle = trimmed[(open + 1)..^1].Trim();
            if (title.Length > 0 && subtitle.Length > 0)
            {
                return (title, subtitle);
            }
        }

        return (trimmed, state.ToString());
    }

    private static string InferIcon(string name)
    {
        string n = name.ToLowerInvariant();
        if (n.Contains("headphone") || n.Contains("headset") || n.Contains("earbud") || n.Contains("airpod"))
        {
            return "Headphones";
        }

        if (n.Contains("monitor") || n.Contains("display") || n.Contains("tv") || n.Contains("hdmi") || n.Contains("nvidia"))
        {
            return "Monitor";
        }

        if (n.Contains("speaker") || n.Contains("realtek") || n.Contains("audio"))
        {
            return "Speakers";
        }

        return "Generic";
    }
}
