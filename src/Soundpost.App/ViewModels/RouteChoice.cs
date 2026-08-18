using CommunityToolkit.Mvvm.ComponentModel;

namespace Soundpost.App.ViewModels;

/// <summary>
/// One option in an app's output-routing picker: either a specific endpoint or "Default" (follow
/// the system default, <see cref="DeviceId"/> null). <see cref="IsActive"/> marks the route the app
/// is currently pinned to, so the picker can highlight it.
/// </summary>
public partial class RouteChoice : ObservableObject
{
    /// <summary>The endpoint id to route to, or null for "follow the system default".</summary>
    public string? DeviceId { get; }

    /// <summary>Short display label — the endpoint's role name (e.g. "Headphones"), or "Default".</summary>
    public string Name { get; }

    /// <summary>Which glyph the pill draws: Headphones, Speakers, Monitor, Generic, or Default.</summary>
    public string IconKind { get; }

    [ObservableProperty]
    private bool _isActive;

    public RouteChoice(string? deviceId, string name, string iconKind, bool isActive)
    {
        DeviceId = deviceId;
        Name = name;
        IconKind = iconKind;
        _isActive = isActive;
    }
}
