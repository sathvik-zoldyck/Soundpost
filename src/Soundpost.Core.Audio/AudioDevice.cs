namespace Soundpost.Core.Audio;

/// <summary>
/// An immutable snapshot of a Windows audio endpoint at a moment in time.
/// Snapshots are safe to hand to any layer; they never hold a live COM handle.
/// </summary>
public sealed record AudioDevice
{
    /// <summary>Stable Windows endpoint id (e.g. "{0.0.0.00000000}.{guid}"). Survives reconnects.</summary>
    public required string Id { get; init; }

    /// <summary>Human-friendly name, e.g. "Headphones (WH-1000XM4)".</summary>
    public required string Name { get; init; }

    /// <summary>Playback or recording.</summary>
    public required AudioDeviceKind Kind { get; init; }

    /// <summary>Availability of the endpoint.</summary>
    public required AudioDeviceState State { get; init; }

    /// <summary>True if this is the default endpoint for the Multimedia/Console role.</summary>
    public bool IsDefault { get; init; }

    /// <summary>True if this is the default endpoint for the Communications role.</summary>
    public bool IsDefaultCommunications { get; init; }

    public override string ToString()
    {
        string tag = IsDefault ? " (default)" : string.Empty;
        string comms = IsDefaultCommunications ? " (comms)" : string.Empty;
        return $"{Name} [{Kind}, {State}]{tag}{comms}";
    }
}
