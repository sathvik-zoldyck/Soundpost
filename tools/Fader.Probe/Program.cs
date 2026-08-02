using Fader.Core.Audio;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Fader audio probe — reading Windows audio endpoints");
Console.WriteLine("===================================================");

using IAudioDeviceService audio = new CoreAudioDeviceService();
using IAudioSessionService sessions = new CoreAudioSessionService();
IDefaultDeviceSwitcher switcher = new PolicyConfigDefaultDeviceSwitcher();

// `dotnet run -- switch "<deviceId>"` actually changes the default output for all roles.
if (args.Length == 2 && args[0].Equals("switch", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Switching default playback to: {args[1]}");
    switcher.SetDefaultForAllRoles(args[1]);
    Console.WriteLine("Done.");
    return;
}

PrintDevices(audio);
PrintSessions(sessions);
SelfTestSwitcher(audio, switcher);

audio.DevicesChanged += (_, change) =>
{
    Console.WriteLine();
    Console.WriteLine($"[change] {change.Kind} {Describe(change)}".TrimEnd());
    // Refresh off the COM callback thread to avoid re-entrancy into the enumerator.
    Task.Run(() =>
    {
        try
        {
            PrintDevices(audio);
            PrintSessions(sessions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  (refresh skipped: {ex.Message})");
        }
    });
};

Console.WriteLine();
Console.WriteLine("Watching for changes — plug/unplug a headset or switch the default device.");
Console.WriteLine("Tip: copy a device id above and run:");
Console.WriteLine("     dotnet run --project tools/Fader.Probe -- switch \"<id>\"");
Console.WriteLine("Press ENTER to exit.");
Console.ReadLine();

static void PrintDevices(IAudioDeviceService audio)
{
    PrintDeviceList("Playback", audio.GetDevices(AudioDeviceKind.Playback));
    PrintDeviceList("Recording", audio.GetDevices(AudioDeviceKind.Recording));

    AudioDevice? defaultPlayback = audio.GetDefaultDevice(AudioDeviceKind.Playback);
    AudioDevice? defaultComms = audio.GetDefaultDevice(AudioDeviceKind.Playback, DeviceRole.Communications);
    AudioDevice? defaultMic = audio.GetDefaultDevice(AudioDeviceKind.Recording);

    Console.WriteLine();
    Console.WriteLine($"  default playback : {defaultPlayback?.Name ?? "(none)"}");
    Console.WriteLine($"  default comms    : {defaultComms?.Name ?? "(none)"}");
    Console.WriteLine($"  default mic      : {defaultMic?.Name ?? "(none)"}");
}

static void PrintDeviceList(string title, IReadOnlyList<AudioDevice> devices)
{
    Console.WriteLine();
    Console.WriteLine($"{title} ({devices.Count}):");
    foreach (AudioDevice device in devices)
    {
        string tag = device.IsDefault
            ? "  *default*"
            : device.IsDefaultCommunications ? "  *comms*" : string.Empty;
        Console.WriteLine($"  - {device.Name}{tag}");
        Console.WriteLine($"      id: {device.Id}");
    }
}

static void PrintSessions(IAudioSessionService sessions)
{
    IReadOnlyList<AudioSession> list = sessions.GetSessions();
    Console.WriteLine();
    Console.WriteLine($"Apps playing on default output ({list.Count}):");
    if (list.Count == 0)
    {
        Console.WriteLine("  (none — start some audio and watch this update)");
        return;
    }

    foreach (AudioSession session in list)
    {
        string mute = session.IsMuted ? " [muted]" : string.Empty;
        Console.WriteLine($"  - {session.DisplayName,-28} {session.Volume * 100,3:0}%{mute}  ({session.State})");
    }
}

static void SelfTestSwitcher(IAudioDeviceService audio, IDefaultDeviceSwitcher switcher)
{
    Console.WriteLine();
    AudioDevice? current = audio.GetDefaultDevice(AudioDeviceKind.Playback);
    if (current is null)
    {
        Console.WriteLine("Switcher self-test: skipped (no default playback device).");
        return;
    }

    try
    {
        // Re-apply the current default: exercises IPolicyConfig::SetDefaultEndpoint end to end
        // without changing what you're actually hearing.
        switcher.SetDefaultForAllRoles(current.Id);
        Console.WriteLine($"Switcher self-test: OK — re-applied '{current.Name}' via IPolicyConfig (no audible change).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Switcher self-test: FAILED — {ex.GetType().Name}: {ex.Message}");
    }
}

static string Describe(AudioDeviceChange change) =>
    change.Role is { } role ? $"({change.DeviceKind} / {role})" : string.Empty;
