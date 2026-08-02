namespace Soundpost.Core.Audio;

/// <summary>
/// An immutable snapshot of a single application's audio session on a device
/// (one row in the per-app mixer).
/// </summary>
public sealed record AudioSession
{
    /// <summary>Owning process id. 0 represents the system sounds session.</summary>
    public required int ProcessId { get; init; }

    /// <summary>Best-effort friendly name (session display name, else process name).</summary>
    public required string DisplayName { get; init; }

    /// <summary>Session volume scalar, 0.0–1.0.</summary>
    public required float Volume { get; init; }

    /// <summary>Whether this app is muted.</summary>
    public required bool IsMuted { get; init; }

    /// <summary>Activity state.</summary>
    public required AudioSessionState State { get; init; }

    /// <summary>Path to the app's icon resource, if the session exposes one.</summary>
    public string? IconPath { get; init; }

    /// <summary>
    /// The session identifier string. Stable per application across launches — the anchor
    /// we'll use to persist per-app volume/routing preferences.
    /// </summary>
    public string SessionIdentifier { get; init; } = string.Empty;

    public override string ToString()
    {
        string mute = IsMuted ? " (muted)" : string.Empty;
        return $"{DisplayName} — {Volume * 100:0}%{mute} [{State}]";
    }
}
