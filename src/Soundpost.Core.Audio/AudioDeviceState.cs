namespace Soundpost.Core.Audio;

/// <summary>
/// Normalized endpoint availability, mirrored from the Windows DEVICE_STATE_* values.
/// Diagnostics uses this to explain, in plain language, why a device can't be used.
/// </summary>
public enum AudioDeviceState
{
    /// <summary>Present and usable.</summary>
    Active,

    /// <summary>Disabled by the user in Sound settings.</summary>
    Disabled,

    /// <summary>The device driver is not present (device removed).</summary>
    NotPresent,

    /// <summary>Physically unplugged from its jack.</summary>
    Unplugged,

    /// <summary>State could not be determined.</summary>
    Unknown,
}
