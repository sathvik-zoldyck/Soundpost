namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// A saved setup (devices + routes + levels). This is the plugin-facing handle; the full model lands
/// with the Scenes/Profiles feature on the roadmap, at which point this record is finalised.
/// </summary>
/// <param name="Id">Stable scene id, used with <see cref="ISoundpostHost.ApplyScene"/>.</param>
/// <param name="Name">Display name.</param>
public readonly record struct Scene(string Id, string Name);
