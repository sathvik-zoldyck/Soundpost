using System.Runtime.InteropServices;

namespace Soundpost.Core.Audio.Interop;

// ---------------------------------------------------------------------------------------------
//  IAudioPolicyConfig — the undocumented WinRT interface behind Windows' "App volume and device
//  preferences" (per-application audio routing). Reached by activating the internal runtime class
//  "Windows.Media.Internal.AudioPolicyConfig" and querying a factory interface whose IID differs
//  between Windows 10 and 11 (the vtable layout is identical across both).
//
//  .NET 5+ removed built-in WinRT marshaling (UnmanagedType.HString / IInspectable), so — unlike
//  EarTrumpet's .NET Framework-era definition — we cannot lean on those. Instead we:
//    * declare the interface as IUnknown-based and model IInspectable's 3 methods as explicit
//      vtable slots, so SetPersistedDefaultAudioEndpoint lands at the correct offset;
//    * pass every HSTRING as a raw IntPtr we create/read/free ourselves via combase.dll.
//
//  The 19 "__incomplete__" methods are never called; they exist only to occupy vtable slots.
//  Quarantined here inside the COM firewall.
// ---------------------------------------------------------------------------------------------

/// <summary>Data-flow direction, matching the Windows EDataFlow enum.</summary>
internal enum EDataFlow
{
    Render = 0,
    Capture = 1,
    All = 2,
}

/// <summary>combase.dll entry points for WinRT activation and manual HSTRING handling.</summary>
internal static class Combase
{
    [DllImport("combase.dll", PreserveSig = false)]
    public static extern void WindowsCreateString(
        [MarshalAs(UnmanagedType.LPWStr)] string sourceString,
        uint length,
        out IntPtr hstring);

    [DllImport("combase.dll")]
    public static extern int WindowsDeleteString(IntPtr hstring);

    [DllImport("combase.dll")]
    public static extern IntPtr WindowsGetStringRawBuffer(IntPtr hstring, out uint length);

    [DllImport("combase.dll", PreserveSig = false)]
    public static extern void RoGetActivationFactory(
        IntPtr activatableClassId,
        [In] ref Guid iid,
        out IntPtr factory);
}

/// <summary>Per-app endpoint factory as exposed on Windows 10 and earlier.</summary>
[Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioPolicyConfigFactoryWin10
{
    // IInspectable (vtable slots 3-5)
    int GetIids();
    int GetRuntimeClassName();
    int GetTrustLevel();

    // IAudioPolicyConfigFactory placeholders (slots 6-24)
    int __incomplete__add_CtxVolumeChange();
    int __incomplete__remove_CtxVolumeChanged();
    int __incomplete__add_RingerVibrateStateChanged();
    int __incomplete__remove_RingerVibrateStateChange();
    int __incomplete__SetVolumeGroupGainForId();
    int __incomplete__GetVolumeGroupGainForId();
    int __incomplete__GetActiveVolumeGroupForEndpointId();
    int __incomplete__GetVolumeGroupsForEndpoint();
    int __incomplete__GetCurrentVolumeContext();
    int __incomplete__SetVolumeGroupMuteForId();
    int __incomplete__GetVolumeGroupMuteForId();
    int __incomplete__SetRingerVibrateState();
    int __incomplete__GetRingerVibrateState();
    int __incomplete__SetPreferredChatApplication();
    int __incomplete__ResetPreferredChatApplication();
    int __incomplete__GetPreferredChatApplication();
    int __incomplete__GetCurrentChatApplications();
    int __incomplete__add_ChatContextChanged();
    int __incomplete__remove_ChatContextChanged();

    // Real methods (slots 25-27). deviceId is an HSTRING passed as a raw IntPtr.
    [PreserveSig]
    int SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);

    [PreserveSig]
    int GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out IntPtr deviceId);

    [PreserveSig]
    int ClearAllPersistedApplicationDefaultEndpoints();
}

/// <summary>Per-app endpoint factory as exposed on Windows 11 (21H2+). Same layout, different IID.</summary>
[Guid("ab3d4648-e242-459f-b02f-541c70306324")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioPolicyConfigFactoryWin11
{
    // IInspectable (vtable slots 3-5)
    int GetIids();
    int GetRuntimeClassName();
    int GetTrustLevel();

    // IAudioPolicyConfigFactory placeholders (slots 6-24)
    int __incomplete__add_CtxVolumeChange();
    int __incomplete__remove_CtxVolumeChanged();
    int __incomplete__add_RingerVibrateStateChanged();
    int __incomplete__remove_RingerVibrateStateChange();
    int __incomplete__SetVolumeGroupGainForId();
    int __incomplete__GetVolumeGroupGainForId();
    int __incomplete__GetActiveVolumeGroupForEndpointId();
    int __incomplete__GetVolumeGroupsForEndpoint();
    int __incomplete__GetCurrentVolumeContext();
    int __incomplete__SetVolumeGroupMuteForId();
    int __incomplete__GetVolumeGroupMuteForId();
    int __incomplete__SetRingerVibrateState();
    int __incomplete__GetRingerVibrateState();
    int __incomplete__SetPreferredChatApplication();
    int __incomplete__ResetPreferredChatApplication();
    int __incomplete__GetPreferredChatApplication();
    int __incomplete__GetCurrentChatApplications();
    int __incomplete__add_ChatContextChanged();
    int __incomplete__remove_ChatContextChanged();

    // Real methods (slots 25-27). deviceId is an HSTRING passed as a raw IntPtr.
    [PreserveSig]
    int SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);

    [PreserveSig]
    int GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out IntPtr deviceId);

    [PreserveSig]
    int ClearAllPersistedApplicationDefaultEndpoints();
}
