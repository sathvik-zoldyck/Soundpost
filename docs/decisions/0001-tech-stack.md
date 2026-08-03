# 0001 — Tech stack: .NET 9 + WPF, layered with a COM firewall

- **Status:** Accepted
- **Date:** 2026-08

## Context

Soundpost is a tray-first, always-running Windows utility that does deep, sometimes *undocumented*
Windows audio COM interop (`IPolicyConfig`, `IAudioPolicyConfig`, WASAPI loopback). It needs a premium
native UI, high reliability, and the largest possible pool of contributors familiar with the domain.

## Decision

- **.NET 9 + C# + WPF** for the app. Audio via **NAudio** plus hand-written interop for the
  undocumented interfaces.
- A **layered architecture** where only `Soundpost.Core.Audio` touches audio COM — the "COM firewall."
- Custom-drawn controls (meters, knobs, visualizer) rather than a heavy control library, for full
  control of the console aesthetic.

## Alternatives considered

- **WinUI 3** — more modern surface, but more lifetime/tray/packaging friction for a utility, and a
  smaller contributor pool for this kind of app.
- **Electron / Tauri** — the deep COM/session/meter work is native territory; a JS/Rust boundary would
  be fragile and slower to build, and the Windows-audio community lives in C#.

## Consequences

- Reliability and interop ergonomics are excellent; contributors familiar with EarTrumpet/SoundSwitch
  feel at home.
- We hand-maintain the visual language (see [STYLE_GUIDE](../../STYLE_GUIDE.md)) instead of inheriting a
  design system — more work, but a distinctive result.
- The firewall keeps the rest of the app testable against fakes and portable in principle.
