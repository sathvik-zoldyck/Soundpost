<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### A mixing desk for your whole PC.

**Switch audio devices instantly. Route apps to the right output. Save scenes. Automate everything — automatically.**

Soundpost is a free, open-source, no-account audio control layer for Windows. It combines the polish of a great volume mixer with a real automation engine and plain-language troubleshooting — so your audio just does the right thing when you plug in your headphones, sit down to game, or join a meeting.

[![Status](https://img.shields.io/badge/status-early%20development-orange)](#project-status)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-0078D4)](#)
[![.NET](https://img.shields.io/badge/.NET-9-512BD4)](#)
[![License](https://img.shields.io/badge/license-GPLv3-green)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen)](CONTRIBUTING.md)

</div>

---

> **⚠️ Project status: early development.** Soundpost is being built in the open, milestone by milestone. It is **not yet installable**. Star the repo to follow along, and see the [Roadmap](#roadmap) for what's landing next. Early contributors welcome — see [CONTRIBUTING](CONTRIBUTING.md).

---

## Why Soundpost exists

Getting audio right on Windows today means juggling **three or four separate tools** that don't talk to each other:

- a mixer for per-app volume,
- a hotkey app to switch the default device,
- a routing tool to send an app to a specific output,
- and a lot of clicking in Settings that **resets after every Windows update**.

None of them *remember your intent*. Plug your Bluetooth headphones in and nothing follows. Something breaks and Windows says nothing about why.

**Soundpost is the missing brain.** One place to control it all — and an automation engine so you rarely have to touch it.

## What it does

Think of your PC as a mixing console. Soundpost gives you the console:

- 🔀 **Instant device switching** — change your default output/input from the tray or a global hotkey.
- 🎚️ **Per-app mixer** — volume, mute, and live meters for every app making sound.
- 🎯 **Per-app routing** — send Spotify to your speakers and Discord to your headset, at the same time.
- 🎬 **Scenes** — save a full audio setup (devices + routes + levels) as *Gaming*, *Movie Night*, *Meeting*, *Night Mode* and apply it in one click.
- 🤖 **Automation** — "when my headphones connect, apply the Headphones scene." Rules trigger on device connect/disconnect (with app-launch, focus, and time triggers coming).
- 🩺 **Plain-language diagnostics** — "Discord is muted," "this app is routed to a device that's unplugged," "another app is holding the device in exclusive mode." With one-click fixes.
- 💾 **Reliable by design** — atomic config saves, automatic backups, and a self-heal loop that restores your setup after Windows shuffles things around.
- 🌈 **Visualizer** — a live "Sound, seen" view that turns whatever's playing into glowing waves, with switchable styles, palettes, and tactile knobs.

**No account. No login. No telemetry. Fully local.**

## What Soundpost is *not* (honesty first)

Windows has real limits, and we won't pretend otherwise:

- **Playing one sound on multiple outputs at once** isn't native to Windows — it needs software loopback (added latency) or a virtual audio driver. It's on the roadmap as an **experimental** module, *not* in the first release, because it's the least reliable feature and reliability is the whole point.
- **Equalizer / DSP** is driver-level territory (that's what [Equalizer APO](https://sourceforge.net/projects/equalizerapo/) does). Soundpost won't reinvent a fragile system-wide EQ; we may *integrate* with existing tools later.
- Some apps only read their audio device when they start, so a routing change may take effect after the app is nudged or restarted. Soundpost **detects and tells you** when that's the case instead of silently failing.

See [`docs/RESEARCH.md`](docs/RESEARCH.md) for the full competitive analysis and Windows-API feasibility study behind these decisions.

## Roadmap

| Milestone | Scope |
|---|---|
| **MVP** | Device switching + hotkeys · per-app mixer · per-app routing · scenes · **auto-switch on device connect/disconnect** · diagnostics · bulletproof persistence |
| **v1** | Full rules engine (app-launch / focus / time triggers) · remember per-app device across reconnects · command palette · onboarding · import from EarTrumpet/SoundSwitch · themes · auto-update |
| **v2** | Multi-output mirroring (experimental) · CLI + declarative config · plugin SDK |
| **Later** | Equalizer APO integration · virtual-device splitting |

Full roadmap: **[ROADMAP.md](ROADMAP.md)**. Progress is tracked in issues and discussions.

## Tech stack

- **.NET 9** · **C#** · **WPF** — a custom dark console UI with custom-drawn controls (meters, knobs, visualizer)
- Audio via [NAudio](https://github.com/naudio/NAudio) (Core Audio) + hand-written COM interop for the parts Windows leaves undocumented (`IPolicyConfig`, `IAudioPolicyConfig`), plus WASAPI loopback + FFT for the visualizer
- MVVM (CommunityToolkit.Mvvm)

Architecture details: [`ARCHITECTURE.md`](ARCHITECTURE.md).

## Building from source

> Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download) on Windows 10/11.

```bash
git clone https://github.com/sathvik-zoldyck/soundpost.git
cd soundpost
dotnet build
```

Run the app:

```bash
dotnet run --project src/Soundpost.App
```

Or exercise the audio layer headlessly (prints devices, reacts to plug/unplug):

```bash
dotnet run --project tools/Soundpost.Probe
```

## Documentation

- [Vision](VISION.md) — what Soundpost is, and the principles behind it
- [Roadmap](ROADMAP.md) — shipped, in progress, and planned
- [Architecture](ARCHITECTURE.md) — how it's built (with a diagram)
- [Plugin SDK](PLUGIN_SDK.md) — extend Soundpost without touching core
- [Style Guide](STYLE_GUIDE.md) — code + visual language
- [Contributing](CONTRIBUTING.md) · [Code of Conduct](CODE_OF_CONDUCT.md) · [Security](SECURITY.md) · [Trademark](TRADEMARK.md)

## Extend Soundpost

The core stays small; the fun lives in the ecosystem:

- 🌈 **[Visualizers](visualizers/)** — build a render style for the "Sound, seen" view.
- 🎭 **[Themes](themes/)** — reskin the console.
- 🔌 **[Plugins](plugins/)** — react to events and automate ([SDK](PLUGIN_SDK.md)).
- 🖼️ **[Showcase](SHOWCASE.md)** — share your setup.

## Contributing

Soundpost is designed to be a friendly, modular open-source project. Whether it's a bug report, a feature idea, a design suggestion, or code — **you're welcome here.** Start with [CONTRIBUTING.md](CONTRIBUTING.md) and look for [`good first issue`](https://github.com/sathvik-zoldyck/soundpost/labels/good%20first%20issue) labels.

## License

[GNU GPLv3](LICENSE) — free to use, study, modify, and share; derivatives stay open. The **Soundpost** name and logo are covered separately by [TRADEMARK.md](TRADEMARK.md). Built with respect for the trails blazed by [EarTrumpet](https://github.com/File-New-Project/EarTrumpet) and [SoundSwitch](https://github.com/Belphemur/SoundSwitch).
