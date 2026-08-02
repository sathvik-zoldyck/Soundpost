namespace Soundpost.Core.Audio;

/// <summary>Activity state of a per-application audio session.</summary>
public enum AudioSessionState
{
    /// <summary>Currently producing audio.</summary>
    Active,

    /// <summary>Open but silent right now.</summary>
    Inactive,

    /// <summary>The owning process has gone; the session is a leftover.</summary>
    Expired,
}
