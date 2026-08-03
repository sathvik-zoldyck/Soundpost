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
| 🔜 | **Tray icon** + minimize-to-tray + single instance |
| 🧭 | Global hotkeys + quick-switch overlay |

## v1 — power without clutter

| Status | Item |
|---|---|
| 🧭 | Full rules engine — triggers on app launch / window focus / time-of-day / manual |
| 🧭 | Remember per-app device across reconnects (fixes the #1 Windows annoyance) |
| 🧭 | **Plugin SDK v1** — load community plugins that react to events ([design](PLUGIN_SDK.md)) |
| 🧭 | More visualizers — **Cymatics (sand)**, **Custom Image**, and a community dropdown |
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
