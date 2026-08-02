using System.Runtime.InteropServices;
using Soundpost.Core.Audio.Interop;

namespace Soundpost.Core.Audio;

/// <summary>
/// <see cref="IAppRoutingService"/> implemented via the undocumented per-app endpoint API.
/// Activates the "Windows.Media.Internal.AudioPolicyConfig" runtime class and selects the
/// correct factory IID for the running Windows version (10 vs 11).
/// </summary>
public sealed class AudioPolicyConfigAppRoutingService : IAppRoutingService
{
    private const string RuntimeClassName = "Windows.Media.Internal.AudioPolicyConfig";

    // Windows 11 is build 22000+. The two OS variants share a vtable but expose different IIDs.
    private static readonly bool IsWindows11 = Environment.OSVersion.Version.Build >= 22000;

    // A routed app should follow the endpoint for every "default" role, mirroring the Windows UI.
    private static readonly DeviceRole[] AllRoles =
    {
        DeviceRole.Console,
        DeviceRole.Multimedia,
        DeviceRole.Communications,
    };

    public void RouteApp(int processId, string deviceId, AudioDeviceKind kind = AudioDeviceKind.Playback)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("A device id is required.", nameof(deviceId));
        }

        SetEndpoint((uint)processId, ToFlow(kind), deviceId);
    }

    public void ResetApp(int processId, AudioDeviceKind kind = AudioDeviceKind.Playback) =>
        // A null/empty endpoint (empty HSTRING) means "follow the system default".
        SetEndpoint((uint)processId, ToFlow(kind), deviceId: null);

    public string? GetAppRoute(int processId, AudioDeviceKind kind = AudioDeviceKind.Playback)
    {
        object factory = Activate();
        EDataFlow flow = ToFlow(kind);

        int hr;
        IntPtr endpointHandle;
        if (IsWindows11)
        {
            hr = ((IAudioPolicyConfigFactoryWin11)factory)
                .GetPersistedDefaultAudioEndpoint((uint)processId, flow, ERole.Multimedia, out endpointHandle);
        }
        else
        {
            hr = ((IAudioPolicyConfigFactoryWin10)factory)
                .GetPersistedDefaultAudioEndpoint((uint)processId, flow, ERole.Multimedia, out endpointHandle);
        }

        if (hr < 0)
        {
            return null;
        }

        try
        {
            string? endpoint = HStringToString(endpointHandle);
            return string.IsNullOrEmpty(endpoint) ? null : endpoint;
        }
        finally
        {
            if (endpointHandle != IntPtr.Zero)
            {
                Combase.WindowsDeleteString(endpointHandle);
            }
        }
    }

    public void ResetAll()
    {
        object factory = Activate();
        int hr = IsWindows11
            ? ((IAudioPolicyConfigFactoryWin11)factory).ClearAllPersistedApplicationDefaultEndpoints()
            : ((IAudioPolicyConfigFactoryWin10)factory).ClearAllPersistedApplicationDefaultEndpoints();

        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
    }

    private void SetEndpoint(uint processId, EDataFlow flow, string? deviceId)
    {
        object factory = Activate();

        IntPtr endpointHandle = IntPtr.Zero;
        try
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                Combase.WindowsCreateString(deviceId, (uint)deviceId.Length, out endpointHandle);
            }

            foreach (DeviceRole role in AllRoles)
            {
                ERole er = ToERole(role);
                int hr = IsWindows11
                    ? ((IAudioPolicyConfigFactoryWin11)factory).SetPersistedDefaultAudioEndpoint(processId, flow, er, endpointHandle)
                    : ((IAudioPolicyConfigFactoryWin10)factory).SetPersistedDefaultAudioEndpoint(processId, flow, er, endpointHandle);

                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }
        finally
        {
            if (endpointHandle != IntPtr.Zero)
            {
                Combase.WindowsDeleteString(endpointHandle);
            }
        }
    }

    private static object Activate()
    {
        Guid iid = IsWindows11
            ? typeof(IAudioPolicyConfigFactoryWin11).GUID
            : typeof(IAudioPolicyConfigFactoryWin10).GUID;

        Combase.WindowsCreateString(RuntimeClassName, (uint)RuntimeClassName.Length, out IntPtr classId);
        try
        {
            Combase.RoGetActivationFactory(classId, ref iid, out IntPtr factoryPtr);
            try
            {
                return Marshal.GetObjectForIUnknown(factoryPtr);
            }
            finally
            {
                // RoGetActivationFactory handed us a reference; the RCW holds its own.
                Marshal.Release(factoryPtr);
            }
        }
        finally
        {
            Combase.WindowsDeleteString(classId);
        }
    }

    private static string? HStringToString(IntPtr hstring)
    {
        if (hstring == IntPtr.Zero)
        {
            return null;
        }

        IntPtr buffer = Combase.WindowsGetStringRawBuffer(hstring, out uint length);
        return buffer == IntPtr.Zero || length == 0
            ? null
            : Marshal.PtrToStringUni(buffer, (int)length);
    }

    private static EDataFlow ToFlow(AudioDeviceKind kind) =>
        kind == AudioDeviceKind.Recording ? EDataFlow.Capture : EDataFlow.Render;

    private static ERole ToERole(DeviceRole role) => role switch
    {
        DeviceRole.Console => ERole.Console,
        DeviceRole.Communications => ERole.Communications,
        _ => ERole.Multimedia,
    };
}
