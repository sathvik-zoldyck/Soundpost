using System.Runtime.InteropServices;

namespace Soundpost.Core.Audio.Interop;

// ---------------------------------------------------------------------------------------------
//  IPolicyConfig — the undocumented interface Windows uses internally to change the default
//  audio endpoint. There is no public header for it; this layout is the community-reverse-
//  engineered definition used by EarTrumpet, SoundSwitch, NirCmd and others, and is stable on
//  Windows 7 through Windows 11 for SetDefaultEndpoint.
//
//  Only SetDefaultEndpoint is called. The earlier vtable entries exist purely to keep the
//  method at the correct slot — their parameters are typed as IntPtr because we never invoke
//  them. This whole file is deliberately quarantined in Soundpost.Core.Audio (the COM firewall).
// ---------------------------------------------------------------------------------------------

/// <summary>Endpoint role, matching the Windows ERole enum.</summary>
internal enum ERole
{
    Console = 0,
    Multimedia = 1,
    Communications = 2,
}

/// <summary>The COM class that implements <see cref="IPolicyConfig"/>.</summary>
[ComImport]
[Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
internal class CPolicyConfigClient
{
}

/// <summary>Undocumented policy-configuration interface (IID f8679f50-...).</summary>
[ComImport]
[Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig
{
    [PreserveSig]
    int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId, out IntPtr format);

    [PreserveSig]
    int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool defaultFormat, out IntPtr format);

    [PreserveSig]
    int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

    [PreserveSig]
    int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId, IntPtr endpointFormat, IntPtr mixFormat);

    [PreserveSig]
    int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool defaultPeriod, out long defaultPeriodValue, out long minimumPeriodValue);

    [PreserveSig]
    int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ref long period);

    [PreserveSig]
    int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string deviceId, out IntPtr mode);

    [PreserveSig]
    int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string deviceId, IntPtr mode);

    [PreserveSig]
    int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool fxStore, IntPtr key, out IntPtr value);

    [PreserveSig]
    int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool fxStore, IntPtr key, IntPtr value);

    /// <summary>Sets <paramref name="deviceId"/> as the default endpoint for the given role.</summary>
    [PreserveSig]
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ERole role);

    [PreserveSig]
    int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool visible);
}
