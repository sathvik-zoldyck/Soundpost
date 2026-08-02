using System.Runtime.InteropServices;
using Soundpost.Core.Audio.Interop;

namespace Soundpost.Core.Audio;

/// <summary>
/// <see cref="IDefaultDeviceSwitcher"/> implemented with the undocumented <c>IPolicyConfig</c>
/// COM interface — the same mechanism the Windows Sound settings UI uses.
/// </summary>
public sealed class PolicyConfigDefaultDeviceSwitcher : IDefaultDeviceSwitcher
{
    private static readonly DeviceRole[] AllRoles =
    {
        DeviceRole.Console,
        DeviceRole.Multimedia,
        DeviceRole.Communications,
    };

    public void SetDefault(string deviceId, params DeviceRole[] roles)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("A device id is required.", nameof(deviceId));
        }

        if (roles is null || roles.Length == 0)
        {
            roles = AllRoles;
        }

        IPolicyConfig? config = null;
        try
        {
            config = (IPolicyConfig)new CPolicyConfigClient();
            foreach (DeviceRole role in roles)
            {
                int hr = config.SetDefaultEndpoint(deviceId, ToERole(role));
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }
        finally
        {
            if (config is not null)
            {
                Marshal.FinalReleaseComObject(config);
            }
        }
    }

    public void SetDefaultForAllRoles(string deviceId) => SetDefault(deviceId, AllRoles);

    private static ERole ToERole(DeviceRole role) => role switch
    {
        DeviceRole.Console => ERole.Console,
        DeviceRole.Communications => ERole.Communications,
        _ => ERole.Multimedia,
    };
}
