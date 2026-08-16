namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// A plugin's manifest, in code. Mirrors the <c>plugin.json</c> that ships next to the assembly and is
/// read before it loads. See <see cref="ISoundpostPlugin.Info"/>.
/// </summary>
public sealed record PluginInfo
{
    /// <summary>Reverse-DNS identity, e.g. <c>"com.you.autoduck"</c>. Stable for the life of the plugin.</summary>
    public required string Id { get; init; }

    /// <summary>Display name shown in the plugins list.</summary>
    public required string Name { get; init; }

    /// <summary>SemVer version string, e.g. <c>"1.0.0"</c>.</summary>
    public required string Version { get; init; }

    /// <summary>Author or organisation.</summary>
    public required string Author { get; init; }

    /// <summary>One-line description of what the plugin does.</summary>
    public string? Description { get; init; }

    /// <summary>Lowest Soundpost version this plugin supports. The host refuses to load it below this.</summary>
    public string MinAppVersion { get; init; } = "0.1.0";
}
