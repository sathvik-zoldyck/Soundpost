# Changelog

All notable changes to Soundpost are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project aims to adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Project scaffold: .NET 9 solution, research + architecture docs, and repo health files
  (issue forms, PR template, CI, Dependabot, Code of Conduct, Security policy).
- `Soundpost.Core.Audio` — the Windows audio layer (the "COM firewall"):
  - Audio endpoint enumeration for playback and recording devices.
  - Default and communications role detection.
  - Live device change events via `IMMNotificationClient` (connect / disconnect / default change).
  - Per-app audio session enumeration with volume, mute, and state.
  - Default-device switching across roles via `IPolicyConfig`.
  - Per-app output/input routing via the undocumented per-app endpoint API (`IAudioPolicyConfig`),
    with separate Windows 10 and Windows 11 interface variants and manual HSTRING / WinRT
    activation interop (built-in WinRT marshaling was removed in .NET 5+).
- `Soundpost.Probe` — a headless harness that prints devices and sessions and reacts to changes,
  with `switch` / `route` / `unroute` commands.
- `Soundpost.App` — the first UI: a WPF dashboard (custom dark theme, MVVM) with one-click
  output-device switching and a live per-app mixer (volume + mute), bound to the audio core.
- Console redesign: a custom dark rack-unit window (borderless chrome, Mixer/Visualizer tabs),
  a scenes bar, output-device cards, per-app channel strips with live segmented peak meters +
  faders, and a master section.
- Live metering (`IAudioMeterService`) — real master and per-session peaks drive the meters.
- Visualizer: WASAPI loopback capture + FFT (`LoopbackAnalyzer`) feeding WPF-drawn styles
  (Ribbon, Spectrum, Radial, Oscilloscope) with draggable knobs (sensitivity, smoothing, glow,
  speed) and switchable palettes.
- Console theme **Forest** (Pine & Amber) — a dark pine-green surface ladder with a warm honey-amber
  accent; selectable live in Settings alongside the existing themes.
- Contributor docs: Vision, Roadmap, Architecture (with diagram), Plugin SDK, Style Guide, Trademark,
  Showcase, ADRs + an RFC template, and scaffolding for community plugins / themes / visualizers / examples.
- Brand: a new Soundpost logo (interlocking-S soundwave) in `assets/`, used in the app and README.

### Changed
- **Relicensed from MIT to GNU GPLv3** — strong copyleft so Soundpost (and everything built on it)
  stays open. See [ADR 0002](docs/decisions/0002-license-gplv3.md); the name/logo are covered by
  [TRADEMARK.md](TRADEMARK.md).
- Bumped NAudio to 2.3.0 (Core Audio property-access performance improvements).
- CI: `actions/checkout@v7` and `actions/setup-dotnet@v6` (clears the Node 20 deprecation warning).
- Pinned `global.json` to a fully-qualified SDK version (`9.0.100`) so `setup-dotnet@v6`'s
  new `global.json` validation accepts it.

_Nothing is released yet — Soundpost is in early development._
