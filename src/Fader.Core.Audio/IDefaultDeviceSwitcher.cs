namespace Fader.Core.Audio;

/// <summary>
/// Changes which endpoint is the Windows default. This is the first action that actually
/// <em>controls</em> audio, and it underpins device switching, scenes, and automation.
/// </summary>
public interface IDefaultDeviceSwitcher
{
    /// <summary>
    /// Sets the given endpoint as the default for the specified roles.
    /// When no roles are given, all three (Console, Multimedia, Communications) are applied.
    /// </summary>
    /// <param name="deviceId">The endpoint id from an <see cref="AudioDevice"/>.</param>
    /// <param name="roles">The roles to change; empty means all roles.</param>
    void SetDefault(string deviceId, params DeviceRole[] roles);

    /// <summary>Sets the endpoint as the default for every role.</summary>
    void SetDefaultForAllRoles(string deviceId);
}
