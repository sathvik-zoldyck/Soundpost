# RFC 0001 — Plugin SDK: abstractions, loader, and trust model

- **Status:** Draft <!-- Draft | Accepted | Rejected | Superseded -->
- **Author(s):** @shibi
- **Created:** 2026-08-16

## Summary

Turn the *proposed* [Plugin SDK](../../PLUGIN_SDK.md) into a real, loadable extension point. This RFC
settles the design questions the SDK doc left open — what ships as a package, how plugins are
discovered and loaded, which thread their callbacks run on, and how far they are trusted — and proposes
a small, phased delivery so the first useful piece (a stable abstractions package) can land without
committing to the whole loader up front.

## Motivation

`PLUGIN_SDK.md` describes a contract — `ISoundpostPlugin`, `ISoundpostHost`, `PluginInfo`, a base class
— but nothing in the app loads a plugin, and the `Soundpost.Plugin.Abstractions` package the doc tells
authors to reference does not exist. So a would-be contributor cannot even start: there is no assembly
to compile against and no folder the app watches.

The roadmap lists **Plugin SDK v1** under "v1 — power without clutter," and both the SDK doc and
[CONTRIBUTING](../../CONTRIBUTING.md) explicitly ask for this to be shaped by an RFC before code. This
is that RFC. It exists to get the *boundary* right — the one thing that is genuinely hard to change
later, because every third-party plugin compiles against it.

## Guide-level explanation

A plugin is a small .NET class library that reacts to Soundpost events and drives actions through a
single `Host` object — its one sanctioned door into the app. Authors override only what they need:

```csharp
public sealed class AutoDuckPlugin : SoundpostPlugin
{
    public override PluginInfo Info => new()
    {
        Id = "com.example.autoduck",
        Name = "Auto-Duck",
        Version = "1.0.0",
        Author = "you",
        Description = "Mutes music while a voice-chat app is playing.",
    };

    public override void OnAudioPeak(AudioPeak peak)
    {
        var discord = Host.GetSessions()
            .FirstOrDefault(s => s.DisplayName.Contains("Discord", StringComparison.OrdinalIgnoreCase));
        if (discord is not null && peak.ProcessId == discord.ProcessId && peak.Level > 0.02f)
        {
            // duck music here via Host.SetAppMute(...)
        }
    }
}
```

They ship the built DLL plus a `plugin.json` manifest in a folder, drop it in
`%AppData%\Soundpost\plugins\<id>\`, and enable it from Settings. No restart: plugins load into a
collectible load context so they can be enabled, disabled, or updated live.

The contract itself is unchanged from `PLUGIN_SDK.md` §1–§2; this RFC does not redesign it, it makes
it real and fills the gaps the doc marked as open.

## Reference-level explanation

### Phase 1 — `Soundpost.Plugin.Abstractions` (the contract, shippable alone)

A new class library containing **only** interfaces, records, and no-op base classes — no loader, no app
dependency, no behaviour change to the running app. This is the piece worth landing first because it is
the frozen surface every plugin binds to.

Contents (verbatim from `PLUGIN_SDK.md` §1–§2, plus the two new records):

- `ISoundpostPlugin`, `SoundpostPlugin` (abstract, no-op defaults)
- `PluginInfo` (record)
- `ISoundpostHost`, `IPluginStorage`, `IPluginLog`
- `AudioPeak(int ProcessId, float Level)`, `Scene(string Id, string Name)` (records)

**Resolved: framework target.** `net9.0-windows`. The shared event models already live in
`Soundpost.Core.Audio` (which is `net9.0-windows`), and a plugin that reacts to audio needs them.

**Resolved: coupling to `Core.Audio`.** The abstractions package **references
`Soundpost.Core.Audio`** and reuses its immutable models (`AudioDevice`, `AudioSession`,
`AudioDeviceKind`, `DeviceRole`) — exactly as the SDK doc states. Those models are plain data; the COM
firewall stays intact because plugins get *values*, never live COM objects, and act only through
`ISoundpostHost`. A parallel set of plugin-facing DTOs was considered and rejected for v1 (a second
model set plus a mapping layer, for no isolation benefit while the models stay data-only).

**Resolved: where `IVisualizerRenderer` lives / `Host.RegisterVisualizer`.** Deferred out of Phase 1.
`IVisualizerRenderer` currently lives in `Soundpost.App` and depends on WPF (`DrawingContext`). Pulling
it into the abstractions package would make the package drag in WPF and would mean moving every
existing built-in renderer. That refactor is worth doing, but it is orthogonal to the event/host
contract and should be its own change (see Unresolved questions). Until then, visualizers are
contributed as built-ins under `visualizers/` (already supported), and `RegisterVisualizer` is omitted
from the v1 host surface rather than shipped half-wired.

Phase 1 also adds a **reference plugin under [`examples/`](../../examples/)** that compiles against the
package — proof the contract is usable, and a copy-paste starting point. It is not loaded by the app
(there is no loader yet); it exists to keep the contract honest.

### Phase 2 — the loader (host implementation in `Soundpost.App`)

- **Discovery.** Scan `%AppData%\Soundpost\plugins\<id>\` and the repo's `plugins/` for folders
  containing a `plugin.json`. The manifest mirrors `PluginInfo` and is read *before* the assembly
  loads, so a plugin can be listed, gated on `MinAppVersion`, and enabled/disabled without ever running
  its code.
- **Loading.** Each plugin gets its own **collectible `AssemblyLoadContext`**, so it can be unloaded
  and reloaded live. The abstractions assembly is shared from the default context (never loaded per
  plugin) so type identity matches across the boundary.
- **Host implementation.** A single `SoundpostHost` adapts the existing services
  (`IAudioDeviceService`, `IAudioSessionService`, `IDefaultDeviceSwitcher`, the metering and
  master-volume services) behind `ISoundpostHost`. Every `act` method is idempotent and logged.
- **Settings UI.** A plugins pane listing discovered plugins with an enable toggle and their manifest
  info, clearly labelling unsigned/community plugins.

**Resolved: threading.** Plugin callbacks run on a **dedicated plugin dispatcher thread**, never the
audio COM thread and never the WPF UI thread. `OnAudioPeak` is throttled to ~20 Hz (as the doc states)
and dispatched there; a slow or throwing plugin therefore cannot stall metering, the UI, or the COM
firewall. `Host` action calls marshal back onto the services' expected context. Every plugin callback
is wrapped so an exception is logged against the plugin id and disables the offender rather than
bringing down the app.

### Phase 3 — hardening

Code-signing awareness and the unsigned-plugin label, then a **separate RFC for a real sandbox**
(out-of-process or a restricted `AssemblyLoadContext`), since in-process full trust is a v1 expedient,
not the end state.

### Manifest (`plugin.json`)

```json
{
  "id": "com.example.autoduck",
  "name": "Auto-Duck",
  "version": "1.0.0",
  "author": "you",
  "description": "Mutes music while a voice-chat app is playing.",
  "minAppVersion": "0.1.0",
  "entry": "AutoDuck.dll"
}
```

## Drawbacks & alternatives

- **In-process full trust is a real risk.** A plugin can do anything the user can. We accept it for v1
  (clearly labelled, opt-in, only plugins the user installs) and commit to a sandbox RFC before this is
  anything but expert-only. This matches the doc's §7.
- **Referencing `Core.Audio` widens the plugin's transitive surface** (it pulls NAudio). Alternative:
  ship plugin-facing DTOs. Rejected for v1 — see above; revisit if the surface proves heavy.
- **Do nothing / keep it a doc.** Then the SDK stays aspirational and no ecosystem forms. The whole
  point of the roadmap item is to let the community automate things we never thought of.
- **Ship the whole loader in one PR.** Higher risk, harder to review, and it freezes the contract and
  the loader design at the same time. Phasing lets the contract stabilise first.

## Unresolved questions

- **Visualizer-as-plugin.** Should `IVisualizerRenderer` move into the abstractions package (making it
  WPF-dependent) so plugins can register renderers at runtime, or stay an app-built-in contract? Leaning
  toward a later, dedicated change once the event/host contract has shipped.
- **Scenes model.** `Scene(Id, Name)` is a placeholder until Scenes/Profiles (roadmap) lands; the scene
  events should be finalised alongside that feature.
- **Manifest signing.** Format and verification of a signature block — deferred to Phase 3.
- **Storage quotas.** Whether `IPluginStorage` needs a size cap per plugin.

## Impact on principles

Measured against the [Vision principles](../../VISION.md#principles):

- **Local-first / no telemetry.** Plugins are local DLLs the user installs; the host offers no network
  or telemetry surface. Third-party plugins are third-party code and are labelled as such — the app
  itself phones home for nothing.
- **Reliability over convenience.** Plugin callbacks are isolated on their own thread and wrapped, so a
  bad plugin is disabled, not fatal. The **COM firewall is preserved**: plugins never touch audio COM,
  only `ISoundpostHost`.
- **Simplicity survives.** One `Host` door, a base class of no-ops, a phased rollout. Nothing new is
  forced on users who never install a plugin.
- **Honesty about Windows.** In-process trust and its risk are stated plainly, not hidden; the sandbox
  is named as future work rather than implied to exist.
