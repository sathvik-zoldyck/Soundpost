# Changelog

All notable changes to Fader are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project aims to adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Project scaffold: .NET 9 solution, research + architecture docs, and repo health files
  (issue forms, PR template, CI, Dependabot, Code of Conduct, Security policy).
- `Fader.Core.Audio` — the Windows audio layer (the "COM firewall"):
  - Audio endpoint enumeration for playback and recording devices.
  - Default and communications role detection.
  - Live device change events via `IMMNotificationClient` (connect / disconnect / default change).
  - Per-app audio session enumeration with volume, mute, and state.
  - Default-device switching across roles via `IPolicyConfig`.
  - Per-app output/input routing via the undocumented per-app endpoint API (`IAudioPolicyConfig`),
    with separate Windows 10 and Windows 11 interface variants and manual HSTRING / WinRT
    activation interop (built-in WinRT marshaling was removed in .NET 5+).
- `Fader.Probe` — a headless harness that prints devices and sessions and reacts to changes,
  with `switch` / `route` / `unroute` commands.
- `Fader.App` — the first UI: a WPF dashboard (custom dark theme, MVVM) with one-click
  output-device switching and a live per-app mixer (volume + mute), bound to the audio core.

### Changed
- Bumped NAudio to 2.3.0 (Core Audio property-access performance improvements).
- CI: `actions/checkout@v7` and `actions/setup-dotnet@v6` (clears the Node 20 deprecation warning).
- Pinned `global.json` to a fully-qualified SDK version (`9.0.100`) so `setup-dotnet@v6`'s
  new `global.json` validation accepts it.

_Nothing is released yet — Fader is in early development._
