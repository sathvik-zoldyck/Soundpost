# Fader — Research & Feasibility Report

This document captures the competitive analysis and Windows-API feasibility study
that shaped Fader's scope. The guiding rule throughout: **be brutally honest about
what Windows actually allows, and choose reliability over cleverness.**

---

## 1. Competitor map

| Tool | Core strength | Loved for | Common complaints / repeated requests |
|---|---|---|---|
| **EarTrumpet** (OSS, C#/WPF) | Best per-app volume mixer + per-app output routing; native tray UI | "Volume Mixer done right," clean, free, trusted | No profiles, no automation, weak hotkeys, doesn't remember per-app device across reconnects. Hotkeys and saved-settings are long-standing requests; the **BetterTrumpet** fork exists specifically to add profiles/themes/media/CLI. |
| **SoundSwitch** (OSS, C#) | Hotkey switching of default device; basic profiles that auto-switch on conditions; CLI | Fast, reliable switching; keyboard-driven | Switching-only — no per-app mixing/routing, minimal UI, no dashboard, no diagnostics. |
| **Voicemeeter** (closed, free) | Powerful virtual mixer + routing matrix + multi-out | Extremely capable for streamers | Steep learning curve, intimidating UI, requires a virtual driver, overkill for "just move my audio." |
| **Equalizer APO / Peace** (OSS) | System-wide EQ/DSP via driver-level APO | Best free EQ on Windows | Breaks on Windows updates, cluttered UI, awkward per-device, can kill audio. |
| **SteelSeries Sonar** (closed) | All-in-one: per-app routing + game/chat/media virtual streams + EQ | Most complete feature set | Bloatware (requires GG client), telemetry, brand lock-in, loses settings after updates, interferes with apps. |
| **Audio Router / CheVolume** | Per-app routing + mirror to multiple outputs | Filled a real gap | Audio Router abandoned/unstable on modern Windows; CheVolume paid + buggy on updates. |
| **Windows built-in** (Volume Mixer + "App volume and device preferences") | Native per-app volume + per-app output | It's built in | Resets after updates, apps don't honor the change until relaunched, no automation, no profiles, no hotkeys, no explanation when something's wrong. |

### Solved / fragmented / missing

- **Solved:** per-app volume (EarTrumpet), device-switch hotkeys (SoundSwitch), EQ (APO), heavy routing (Voicemeeter).
- **Fragmented:** achieving mixing + hotkeys + profiles + switching requires **3–4 tools** that don't cooperate.
- **Missing (the gap Fader fills):** a **polished, open, driver-free "brain"** combining EarTrumpet-grade per-app control with SoundSwitch-grade switching **plus** a real automation/rules engine, profiles/scenes, and plain-language diagnostics — with **reliability** (survives updates, remembers intent, self-heals) as the headline.

---

## 2. Windows audio reality — feasible vs. magic

### ✅ Cleanly feasible, no virtual driver, reliable

| Capability | Mechanism | Notes |
|---|---|---|
| Enumerate devices + live connect/disconnect/default-change events | `IMMDeviceEnumerator` + `IMMNotificationClient` | Rock solid. This is what makes automation possible. |
| Switch the system default device (all roles) | undocumented `IPolicyConfig::SetDefaultEndpoint` | Battle-tested (EarTrumpet, SoundSwitch, NirCmd). |
| Per-app output/input routing | undocumented `IAudioPolicyConfig::SetPersistedDefaultAudioEndpoint(pid, flow, role, deviceId)` | Backend of Windows' "App volume and device preferences." Interface **differs between Win10 and Win11** — must handle both. Apps that read their device only at stream start may need a nudge/relaunch. |
| Per-app + per-device volume/mute/meters | `IAudioSessionManager2` + `ISimpleAudioVolume` + `IAudioMeterInformation` | Solid. |
| Session enumeration (app name, icon, PID, state) | `IAudioSessionControl2` | Solid. |
| Global hotkeys | `RegisterHotKey` | Solid. |
| Automation triggers | notification client (device), process/ETW watch (app launch/exit), WinEvent hook (focus), scheduler (time), hotkey/manual | Solid. |
| Diagnostics | reading device/session states | Everything needed is readable — high value, low risk. |

### ⚠️ Needs loopback or a virtual driver — reliability risk, deferred

- **Mirror one source → multiple outputs simultaneously.** Windows routes one stream to one endpoint. Options: WASAPI **loopback capture + re-render** to N endpoints (no driver, but adds latency and needs sample-rate/clock-drift correction), or bundle a **virtual cable** driver (install + signing friction). This is the least clean feature → **not MVP**; ships later as an explicitly experimental module.

### ❌ Out of scope — don't reinvent

- **EQ/DSP/effects** → driver-level APO (Equalizer APO's fragile territory). Don't build; maybe *integrate* later.
- **Spatial audio, mic noise suppression, game/chat virtual splitting** → driver-level or ML; out of scope for a reliable v1.

---

## 3. Positioning

> **The reliable, open "autopilot" for your audio.** EarTrumpet's polish + SoundSwitch's switching + a real rules engine + plain-language diagnostics — driver-free, no bloat, no telemetry, MIT-licensed.

**Brand metaphor: a mixing desk for your whole PC.** A real console already has channel routing, scene snapshots, and automation — which maps cleanly onto Fader's routing, profiles/scenes, and rules.

**Simplification adopted:** *scenes* and *profiles* are the same primitive — a named bundle of desired state. A *rule* is simply how a profile gets applied (hotkey, device event, app launch, time). One small mental model, clean UI.

---

## 4. Feature ranking (impact × difficulty)

| Feature | Impact | Difficulty | Bucket |
|---|---|---|---|
| Instant default-device switch (tray + hotkey + overlay) | High | Low–Med | MVP |
| Per-app mixer: volume, mute, meters | High | Med | MVP |
| Per-app output routing | High | Med | MVP |
| Live device/app/route dashboard | High | Med | MVP |
| Profiles/scenes (apply on demand/hotkey) | High | Med | MVP |
| Auto-apply on device connect/disconnect | High | Med | MVP |
| Plain-language diagnostics | High | Med–High | MVP |
| Bulletproof persistence + self-heal | High | Med | MVP |
| Full rules engine (app-launch/focus/time/manual) | High | Med–High | v1 |
| Remember per-app device across reconnects | High | Med–High | v1 |
| Command palette + onboarding + importers | Med | Med | v1 |
| Themes / Mica / polish | Med | Med | v1 |
| Multi-output mirroring (experimental) | Med | High | v2 |
| CLI + declarative config | Med | Med | v2 |
| Plugin SDK | Med | High | v2 |
| EQ APO integration / virtual splitting | Low–Med | High | later |

**Deliberately cut:** built-in EQ (fragile APO), day-one plugin system (premature), mirroring in MVP (least reliable feature — would undermine the stability brand).

---

## 5. MVP scope

The reliable "brain," no mirroring, no EQ:

1. Tray app + main dashboard (devices, apps, routes, states at a glance).
2. Device switching (all roles) + global hotkeys + quick-switch overlay.
3. Per-app mixer (volume/mute/route) — EarTrumpet parity baseline.
4. Profiles/Scenes — create, save, apply, hotkey.
5. **One automation trigger done impeccably:** device connect/disconnect → apply a profile.
6. Plain-language diagnostics.
7. Rock-solid persistence (atomic writes + backups + migration) + logs + self-heal reconcile.

## 6. Roadmap

- **v1:** full rules engine, per-app device memory across reconnects, command palette, onboarding + importers, themes, auto-update.
- **v2:** multi-output mirroring (experimental), CLI + declarative config, plugin SDK.
- **Later:** EQ-APO integration, game/chat virtual splitting.

---

## 7. Stack & architecture summary

**.NET 9 + C# + WPF + WPF-UI.** Audio via NAudio (Core Audio) + hand-written interop for the undocumented interfaces. MVVM (CommunityToolkit.Mvvm), DI/host (Microsoft.Extensions.Hosting), logging (Serilog). Packaged as MSIX + portable zip + winget. **License: MIT.**

WPF over WinUI 3 for a tray-first, always-running utility doing deep COM interop: more reliable lifetime/tray/hotkey story, the largest contributor pool, and precedent (EarTrumpet, SoundSwitch). WPF-UI keeps it visually modern.

Full architecture: [`ARCHITECTURE.md`](ARCHITECTURE.md).

---

## Sources

Competitive and API research drew on, among others:

- EarTrumpet — feature requests for [hotkeys](https://github.com/File-New-Project/EarTrumpet/issues/283), [saved settings](https://github.com/File-New-Project/EarTrumpet/discussions/649); the [BetterTrumpet](https://github.com/xammen/BetterTrumpet) fork.
- SoundSwitch — [profiles/auto-switch docs](https://soundswitch.aaflalo.me/usage/profiles); Win11 audio-switching [interop fix](https://github.com/Belphemur/SoundSwitch/commit/40f5ba119a41d04b8057e75146345c84dfe238c3).
- `IAudioPolicyConfig` per-app endpoint — [Microsoft: Default Audio Endpoint Selection](https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/default-audio-endpoint-selection); community usage in SoundSwitch and stream-controller projects.
- Windows per-app routing resets/relaunch behavior — [Microsoft Q&A](https://learn.microsoft.com/en-us/answers/questions/5883971/set-sound-output-they-different-devices-for-each-a).
- Equalizer APO / Peace fragility — [Peace usage FAQ & reviews](https://sourceforge.net/p/peace-equalizer-apo-extension/wiki/Usage%20FAQ/).
- Voicemeeter learning curve — [AlternativeTo discussions](https://alternativeto.net/software/voicemeeter/).
- SteelSeries software reliability — [Trustpilot reviews](https://ca.trustpilot.com/review/www.steelseries.com).
- Multi-output limitations — [community threads on playing to multiple devices](https://appuals.com/output-audio-multiple-devices-windows-10/).
