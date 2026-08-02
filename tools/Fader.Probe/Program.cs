using Fader.Core.Audio;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Fader audio probe — reading Windows audio endpoints");
Console.WriteLine("===================================================");

using IAudioDeviceService audio = new CoreAudioDeviceService();
using IAudioSessionService sessions = new CoreAudioSessionService();
IDefaultDeviceSwitcher switcher = new PolicyConfigDefaultDeviceSwitcher();
IAppRoutingService routing = new AudioPolicyConfigAppRoutingService();

// Command mode: perform an action and exit.
if (args.Length >= 1 && TryRunCommand(args, switcher, routing))
{
    return;
}

PrintDevices(audio);
PrintSessions(sessions, routing, audio);
SelfTestSwitcher(audio, switcher);
SelfTestRouting(routing);

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
            PrintSessions(sessions, routing, audio);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  (refresh skipped: {ex.Message})");
        }
    });
};

Console.WriteLine();
Console.WriteLine("Watching for changes — plug/unplug a headset or switch the default device.");
Console.WriteLine("Commands (copy an id from above):");
Console.WriteLine("  dotnet run --project tools/Fader.Probe -- switch \"<deviceId>\"");
Console.WriteLine("  dotnet run --project tools/Fader.Probe -- route <pid> \"<deviceId>\"");
Console.WriteLine("  dotnet run --project tools/Fader.Probe -- unroute <pid>");
Console.WriteLine("Press ENTER to exit.");
Console.ReadLine();

static bool TryRunCommand(string[] args, IDefaultDeviceSwitcher switcher, IAppRoutingService routing)
{
    string command = args[0].ToLowerInvariant();
    switch (command)
    {
        case "switch" when args.Length == 2:
            Console.WriteLine($"Switching default playback to: {args[1]}");
            switcher.SetDefaultForAllRoles(args[1]);
            Console.WriteLine("Done.");
            return true;

        case "route" when args.Length == 3 && int.TryParse(args[1], out int routePid):
            Console.WriteLine($"Routing PID {routePid} -> {args[2]}");
            routing.RouteApp(routePid, args[2]);
            Console.WriteLine("Done. (The app may need to restart its audio stream to honor it.)");
            return true;

        case "unroute" when args.Length == 2 && int.TryParse(args[1], out int unroutePid):
            routing.ResetApp(unroutePid);
            Console.WriteLine($"PID {unroutePid} reset to follow the system default.");
            return true;

        default:
            Console.WriteLine($"Unknown command: {string.Join(' ', args)}");
            return true;
    }
}

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

static void PrintSessions(IAudioSessionService sessions, IAppRoutingService routing, IAudioDeviceService audio)
{
    // Map endpoint ids to names so a per-app route reads as a device, not a GUID.
    var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (AudioDevice device in audio.GetDevices(AudioDeviceKind.Playback, includeUnavailable: true))
    {
        names[device.Id] = device.Name;
    }

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
        string routeText = "follows default";
        try
        {
            string? route = routing.GetAppRoute(session.ProcessId);
            if (route is not null)
            {
                routeText = "-> " + (names.TryGetValue(route, out string? name) ? name : route);
            }
        }
        catch
        {
            routeText = "route unknown";
        }

        Console.WriteLine($"  - {session.DisplayName,-28} {session.Volume * 100,3:0}%{mute}  ({session.State})  {routeText}");
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

static void SelfTestRouting(IAppRoutingService routing)
{
    try
    {
        // Read *this* process's route: proves the WinRT activation + vtable are correct,
        // with zero side effects (we never change anyone's routing).
        string? route = routing.GetAppRoute(Environment.ProcessId);
        Console.WriteLine($"Routing self-test: OK — per-app endpoint API reachable (this process: {route ?? "follows default"}).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Routing self-test: FAILED — {ex.GetType().Name}: {ex.Message}");
    }
}

static string Describe(AudioDeviceChange change) =>
    change.Role is { } role ? $"({change.DeviceKind} / {role})" : string.Empty;
