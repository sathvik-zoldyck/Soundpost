# Security Policy

Fader runs entirely on your machine. It has **no account system, no server, no
network calls, and no telemetry.** Your configuration lives locally on disk.

## What Fader touches

- **Windows audio APIs** (Core Audio / WASAPI) to enumerate and control devices,
  sessions, and default endpoints.
- Some of these APIs are **undocumented** (`IPolicyConfig`, `IAudioPolicyConfig`).
  Fader uses them the same way Windows' own Settings UI does. Interop code is
  isolated in `Fader.Core.Audio` and commented for auditability.
- **Local config files** under `%AppData%\Fader` (profiles, rules, settings, logs).

Fader does **not** capture audio content, record microphones, read other apps'
data, or transmit anything off your device.

## Reporting a vulnerability

If you discover a security issue, please **do not open a public issue.** Instead,
open a private security advisory via the repository's *Security* tab, or contact
the maintainer directly. We'll acknowledge within a few days and work with you on
a fix and coordinated disclosure.

## Supported versions

During early development, only the latest `main` receives fixes. A supported
release policy will be published with the first tagged release.
