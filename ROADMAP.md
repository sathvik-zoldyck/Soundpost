# Roadmap

This is a living document. Priorities are shaped by the [Vision](VISION.md), by real user pain
(open an [issue](../../issues) or [discussion](../../discussions)), and by [RFCs](docs/rfcs/).
It is a direction, not a promise of dates.

**Legend:** ✅ shipped · 🚧 in progress · 🔜 next · 🧭 planned · 💭 exploring

---

## The core promise (MVP): a reliable audio brain

The backbone. Everything else sits on top of this working flawlessly.

| Status | Item |
|---|---|
| ✅ | Device enumeration + live connect/disconnect/default-change events (`Core.Audio`) |
| ✅ | Instant default-device switching across roles (`IPolicyConfig`) |
| ✅ | Per-app sessions: volume, mute, state |
| ✅ | Per-app output routing (`IAudioPolicyConfig`, Win10 + Win11 variants) |
| ✅ | Live per-session + master metering |
| ✅ | Console UI: custom chrome, device cards, channel strips, master section |
| ✅ | Visualizer: WASAPI loopback + FFT, live styles + knobs |
| 🚧 | **Persistence** — atomic config store, backups, schema migration (`%AppData%\Soundpost`) |
| 🔜 | **Scenes/Profiles** — save a full setup (devices + routes + levels) and apply it |
| 🔜 | **Automation v1** — one trigger done impeccably: device connect/disconnect → apply a scene |
| 🔜 | **Plain-language diagnostics** — "this app is routed to an unplugged device," with one-click fixes |
| 🔜 | **Self-heal reconcile** — re-apply your intent after a Windows update shuffles things |
| ✅ | **Tray icon** + minimize-to-tray + single instance |
| ✅ | **Quick Panel** — a compact tray flyout for the moves you make mid-meeting, without opening the console ([plan](#quick-panel)) |
| 🧭 | Global hotkeys + quick-switch overlay |

## v1 — power without clutter

| Status | Item |
|---|---|
| 🧭 | Full rules engine — triggers on app launch / window focus / time-of-day / manual |
| 🧭 | Remember per-app device across reconnects (fixes the #1 Windows annoyance) |
| 🧭 | **Custom Sound Templates** — user-named preference bundles (per-app volume + routing + device + visualizer style/palette), built and named in a friendly UI and applied in one click. Keep as many as you like (*Relaxing*, *Gaming*, *Focus*…). Extends Scenes into full one-click "setup macros." |
| 🧭 | **Plugin SDK v1** — load community plugins that react to events ([design](PLUGIN_SDK.md)) |
| 🚧 | More visualizers — **Cymatics (sand)** + **Custom Image**, plus a **dropdown** so contributors can add their own (Ribbon / Spectrum / Radial / Oscilloscope already shipped; Aurora removed) |
| 🧭 | Themes — reskin the console; community theme folder |
| 🧭 | Command palette + first-run onboarding |
| 🧭 | Import from EarTrumpet / SoundSwitch |
| 🧭 | Auto-update (winget + MSIX) |

## v2 — the ecosystem

| Status | Item |
|---|---|
| 💭 | Multi-output mirroring (experimental, loopback-based — clearly labeled with its latency) |
| 💭 | CLI + declarative config file for power users |
| 💭 | Plugin/theme/visualizer registry — discover and install community work in-app |
| 💭 | Equalizer APO *integration* (detect + drive it, don't reinvent DSP) |

## Quick Panel

You're in a meeting and one app is too loud. Opening a 1220×740 console to drag one slider is
absurd. The Quick Panel is the answer: a small flyout from the tray icon holding only the moves you
make without thinking.

**What's in it**

| | |
|---|---|
| Master volume + mute | the single most common action |
| Output switcher | a compact device list — one click to change default |
| Per-app rows | icon, slider, mute — the meeting case: silence one app fast |
| "Open Soundpost" | escape hatch to the full console |

**What's deliberately out:** visualizer, knobs, routing, scenes, settings. Everything that needs room
to think stays in the console. If the panel grows a scrollbar, it has failed.

**How it's built**

- Tray icon via [H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon) — the maintained continuation
  of Hardcodet's NotifyIcon, with .NET 6+ and flyout support.
- `QuickPanelWindow`: `WindowStyle=None`, `ShowInTaskbar=False`, `Topmost`, closing on `Deactivated`
  so it behaves like the Windows volume flyout.
- Positioned against the taskbar's work area and clamped to the monitor, so it stays correct on
  multi-monitor setups and with a left- or top-docked taskbar.
- Reuses `MainViewModel` unchanged — the same devices, sessions and master volume, a second view
  over one source of truth. No duplicate polling.
- Meter polling is gated on panel visibility as well as section, for the same reason it's gated on
  the visualizer: those calls cross into Core Audio COM on the UI thread and are not free.

**Delivery order:** tray icon + minimize-to-tray + single instance first, then the flyout on top of it.

## Explicitly not planned

- A DAW, or a system-wide EQ/DSP engine of our own.
- Accounts, cloud sync, subscriptions, telemetry.
- Anything that phones home.

---

## How to influence the roadmap

- **Small idea or bug?** Open an [issue](../../issues).
- **Bigger change?** Open a [discussion](../../discussions), then a short [RFC](docs/rfcs/) if it
  affects architecture or the plugin API.
- **Want to build one of these?** Comment on the tracking issue or say hi in discussions — we'll
  help you scope it. See [CONTRIBUTING.md](CONTRIBUTING.md).
