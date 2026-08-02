<div align="center">

# 🎛️ Fader

### A mixing desk for your whole PC.

**Switch audio devices instantly. Route apps to the right output. Save scenes. Automate everything — automatically.**

Fader is a free, open-source, no-account audio control layer for Windows. It combines the polish of a great volume mixer with a real automation engine and plain-language troubleshooting — so your audio just does the right thing when you plug in your headphones, sit down to game, or join a meeting.

[![Status](https://img.shields.io/badge/status-early%20development-orange)](#project-status)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-0078D4)](#)
[![.NET](https://img.shields.io/badge/.NET-9-512BD4)](#)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen)](CONTRIBUTING.md)

</div>

---

> **⚠️ Project status: early development.** Fader is being built in the open, milestone by milestone. It is **not yet installable**. Star the repo to follow along, and see the [Roadmap](#roadmap) for what's landing next. Early contributors welcome — see [CONTRIBUTING](CONTRIBUTING.md).

---

## Why Fader exists

Getting audio right on Windows today means juggling **three or four separate tools** that don't talk to each other:

- a mixer for per-app volume,
- a hotkey app to switch the default device,
- a routing tool to send an app to a specific output,
- and a lot of clicking in Settings that **resets after every Windows update**.

None of them *remember your intent*. Plug your Bluetooth headphones in and nothing follows. Something breaks and Windows says nothing about why.

**Fader is the missing brain.** One place to control it all — and an automation engine so you rarely have to touch it.

## What it does

Think of your PC as a mixing console. Fader gives you the console:

- 🔀 **Instant device switching** — change your default output/input from the tray or a global hotkey.
- 🎚️ **Per-app mixer** — volume, mute, and live meters for every app making sound.
- 🎯 **Per-app routing** — send Spotify to your speakers and Discord to your headset, at the same time.
- 🎬 **Scenes** — save a full audio setup (devices + routes + levels) as *Gaming*, *Movie Night*, *Meeting*, *Night Mode* and apply it in one click.
- 🤖 **Automation** — "when my headphones connect, apply the Headphones scene." Rules trigger on device connect/disconnect (with app-launch, focus, and time triggers coming).
- 🩺 **Plain-language diagnostics** — "Discord is muted," "this app is routed to a device that's unplugged," "another app is holding the device in exclusive mode." With one-click fixes.
- 💾 **Reliable by design** — atomic config saves, automatic backups, and a self-heal loop that restores your setup after Windows shuffles things around.

**No account. No login. No telemetry. Fully local.**

## What Fader is *not* (honesty first)

Windows has real limits, and we won't pretend otherwise:

- **Playing one sound on multiple outputs at once** isn't native to Windows — it needs software loopback (added latency) or a virtual audio driver. It's on the roadmap as an **experimental** module, *not* in the first release, because it's the least reliable feature and reliability is the whole point.
- **Equalizer / DSP** is driver-level territory (that's what [Equalizer APO](https://sourceforge.net/projects/equalizerapo/) does). Fader won't reinvent a fragile system-wide EQ; we may *integrate* with existing tools later.
- Some apps only read their audio device when they start, so a routing change may take effect after the app is nudged or restarted. Fader **detects and tells you** when that's the case instead of silently failing.

See [`docs/RESEARCH.md`](docs/RESEARCH.md) for the full competitive analysis and Windows-API feasibility study behind these decisions.

## Roadmap

| Milestone | Scope |
|---|---|
| **MVP** | Device switching + hotkeys · per-app mixer · per-app routing · scenes · **auto-switch on device connect/disconnect** · diagnostics · bulletproof persistence |
| **v1** | Full rules engine (app-launch / focus / time triggers) · remember per-app device across reconnects · command palette · onboarding · import from EarTrumpet/SoundSwitch · themes · auto-update |
| **v2** | Multi-output mirroring (experimental) · CLI + declarative config · plugin SDK |
| **Later** | Equalizer APO integration · virtual-device splitting |

Progress is tracked in the repo's issues and project board.

## Tech stack

- **.NET 9** · **C#** · **WPF** with [WPF-UI](https://github.com/lepoco/wpfui) (Fluent / Mica)
- Audio via [NAudio](https://github.com/naudio/NAudio) (Core Audio APIs) + hand-written COM interop for the parts Windows leaves undocumented (`IPolicyConfig`, `IAudioPolicyConfig`)
- MVVM (CommunityToolkit.Mvvm) · DI/host (Microsoft.Extensions.Hosting) · logging (Serilog)

Architecture details: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Building from source

> Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download) on Windows 10/11.

```bash
git clone https://github.com/<your-org>/fader.git
cd fader
dotnet build
```

To run the headless audio probe (prints your devices and reacts to plug/unplug — the first thing that works):

```bash
dotnet run --project tools/Fader.Probe
```

## Contributing

Fader is designed to be a friendly, modular open-source project. Whether it's a bug report, a feature idea, a design suggestion, or code — **you're welcome here.** Start with [CONTRIBUTING.md](CONTRIBUTING.md) and look for [`good first issue`](https://github.com/<your-org>/fader/labels/good%20first%20issue) labels.

## License

[MIT](LICENSE) — do anything you like, just keep the notice. Built with respect for the trails blazed by [EarTrumpet](https://github.com/File-New-Project/EarTrumpet) and [SoundSwitch](https://github.com/Belphemur/SoundSwitch).
