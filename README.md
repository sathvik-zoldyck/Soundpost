<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### One center. Every sound.

**One console for Windows audio: switch your output device, mix every app, and see your sound — in one place. Local-first, no account, no telemetry.**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#get-it)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[Roadmap](ROADMAP.md) · [Architecture](ARCHITECTURE.md) · [Plugin SDK](PLUGIN_SDK.md) · [Contribute](CONTRIBUTING.md) · [Discussions](../../discussions)**

English · [简体中文](README.zh.md) · [Español](README.es.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [日本語](README.ja.md) · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="The Soundpost dashboard: master volume dial, playback device switcher, per-app mixer, and live output meters" width="880" />

</div>

---

Windows scatters your audio across a volume flyout, a device menu, a sound control panel, and a handful of third-party utilities that do not talk to each other — and none of them remember what you wanted. **Soundpost is the missing center.** Every device and every app flows into one console: you switch, mix, and route it, and it lands on the right output. And when you just want to listen, the visualizer turns whatever is playing into something worth watching.

## Features

- **Instant device switching.** Change your default output or input in one click.
- **Per-app mixer.** Volume, mute, and live meters for every application making sound.
- **Live output metering.** Master and per-app peak meters with real ballistics.
- **Visualizer.** Seven live styles — Ribbon, Aurora, Spectrum, Radial, Oscilloscope, Cymatics, and a Custom Image mode — that react to your audio at 60 fps, with adjustable sensitivity, smoothing, glow, and palette.
- **Fullscreen overlay.** Pop the visualizer out over a music video with solid, dimmed, or fully transparent backdrops.
- **Quick Panel.** A compact tray flyout for the moves you make mid-meeting, without opening the full console.
- **Four themes.** Indigo, Black & Red, Rich Gold, and Cherry Blossom — switchable live from Settings.
- **Local and private.** No account, no cloud, no telemetry. Everything stays on your machine.

## See it

<div align="center">

<img src="assets/media/themes.png" alt="Soundpost in all four themes: Indigo, Black and Red, Rich Gold, and Cherry Blossom" width="880" />

<sub><b>Four themes, switched live.</b> Indigo, Black &amp; Red, Rich Gold, Cherry Blossom.</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="The Quick Panel tray flyout with master volume, output switching, and per-app controls" width="320" />

<sub><b>Quick Panel.</b> Master volume, output switching, and per-app mute — straight from the tray.</sub>

</div>

## Get it

Soundpost targets **Windows 10 and 11**. Grab a build from [Releases](../../releases) once one is published, or build from source:

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

You need the [.NET 9 SDK](https://dotnet.microsoft.com/download). A one-file, self-contained `Soundpost.exe` (no .NET install required) is produced by the release workflow.

## How it works

Every app and device flows into one center; you route, mix, and automate it, and it lands on the right output. A single Core Audio layer wraps the Windows COM APIs so the rest of the app never touches them directly, which keeps the console responsive and the audio handling isolated and testable.

## Extend it

Soundpost is built to be added to.

- **Visualizers.** A style is one class implementing `IVisualizerRenderer` — see [visualizers/](visualizers/). Write it, register it, and it appears in the style bar.
- **Themes.** Palettes are self-contained dictionaries; a new theme is a new file plus a swatch.
- **Plugins.** An event-driven plugin surface is on the roadmap — see [PLUGIN_SDK.md](PLUGIN_SDK.md).

## Roadmap

Shipping now: device switching, per-app mixer and meters, the visualizer, the tray and Quick Panel, persistence, and themes. Next: scenes and profiles, an automation layer, per-app routing, and plain-language diagnostics. The full plan lives in [ROADMAP.md](ROADMAP.md).

## Contributing

Contributions are welcome, from a new visualizer to a bug fix. Start with [CONTRIBUTING.md](CONTRIBUTING.md), open an [issue](../../issues), or say hello in [Discussions](../../discussions). If Soundpost is useful to you, a star helps other people find it.

## License

[GPLv3](LICENSE). Free and open source.
