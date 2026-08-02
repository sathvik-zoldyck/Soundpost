# Soundpost — Architecture

Soundpost is built as layers with a **single source of truth** and **one-way data flow**.
The design goals, in priority order: **reliability, observability, testability, and
keeping the scary Windows COM code sealed off from everything else.**

## Layers

```
             Windows Core Audio / WASAPI (COM, some undocumented)
                                  │
        ┌─────────────────────────▼─────────────────────────┐
        │  Soundpost.Core.Audio                                  │
        │  The ONLY layer allowed to touch audio COM.        │
        │  - Device enumeration + IMMNotificationClient      │
        │  - Session enumeration + volume/mute/meters        │
        │  - Default-device switch (IPolicyConfig)           │
        │  - Per-app routing (IAudioPolicyConfig)            │
        │  Exposes normalized models + events + services.    │
        └─────────────────────────┬─────────────────────────┘
                                  │  normalized events / models
        ┌─────────────────────────▼─────────────────────────┐
        │  Soundpost.Core.Engine                                 │
        │  - AudioState store (single source of truth)       │
        │  - RulesEngine (trigger → condition → action)      │
        │  - ProfileManager (scenes = named state bundles)   │
        │  - DiagnosticsEngine (plain-language checks)        │
        │  - Reconcile/self-heal loop                        │
        └───────────────┬───────────────────┬───────────────┘
                        │                   │
        ┌───────────────▼──────┐   ┌────────▼──────────────────┐
        │ Soundpost.Core.Persistence│   │ Soundpost.App (WPF/MVVM)       │
        │ - Atomic JSON config  │   │ - Tray + quick-switch      │
        │ - Backups + migrations│   │ - Dashboard / Mixer /      │
        │ - Serilog logging     │   │   Profiles / Rules /       │
        └───────────────────────┘   │   Diagnostics / Settings   │
                                    │ Observes state; dispatches │
                                    │ Actions. Never touches COM.│
                                    └────────────────────────────┘

  Soundpost.App.Host = composition root: DI, single-instance, tray lifetime,
                   hotkey service, background workers.
  Plugin SDK (v2): ITrigger / IAction / IDiagnostic contracts.
```

## Data flow (one direction)

1. Windows raises a COM event (device added, default changed, session created…).
2. `Core.Audio` normalizes it into a plain model and raises a .NET event.
3. `Core.Engine` updates the **AudioState** store and runs the `RulesEngine` +
   `DiagnosticsEngine` against the new state.
4. The **UI observes** AudioState (via `INotifyPropertyChanged` / messenger) and
   re-renders. The UI never mutates audio directly.
5. A user action (or a rule firing) **dispatches an Action** (e.g. `SetDefaultDevice`,
   `RouteApp`, `ApplyProfile`).
6. The Engine executes the Action **through `Core.Audio`**, which changes Windows
   state → which raises an event → back to step 1. The loop closes; state stays true.

## Key design decisions

### The COM firewall
Only `Soundpost.Core.Audio` references audio COM. Undocumented interfaces (`IPolicyConfig`,
`IAudioPolicyConfig`) live behind small, documented service interfaces
(`IDefaultDeviceService`, `IAppRoutingService`). This means:
- The rest of the app is portable/testable against fakes.
- Windows 10 vs 11 interface differences are handled in exactly one place.

### AudioState as single source of truth
A snapshot of the world: devices, default endpoints per role, active sessions, and
their volumes/routes. The UI is a pure function of this state. Rules evaluate on state
transitions. This is what makes behavior predictable and debuggable.

### Actions are commands
Every mutation is an explicit, logged, **idempotent** Action. Idempotency matters
because the reconcile loop may re-issue an Action to restore desired state. Actions
report success/failure, and failures surface in Diagnostics rather than failing silently.

### Reconcile / self-heal loop
Users' *desired* state (their profiles/rules) is persisted. Windows' *actual* state
drifts (updates reset per-app routing; devices come and go). A periodic + event-driven
reconcile compares desired vs. actual and re-applies the difference. This is the
concrete mechanism behind "it remembers your intent."

### Persistence is defensive
- Writes go to a temp file then atomically rename over the target.
- Every save keeps a timestamped backup; a corrupt config auto-restores from the newest good one.
- Config carries a schema version; migrations run forward on load.
- Logs (Serilog, rolling files) live next to config under `%AppData%\Soundpost`.

## Projects

| Project | Type | Responsibility |
|---|---|---|
| `Soundpost.Core.Audio` | classlib (`net9.0-windows`) | All Windows audio interop; normalized models/services. |
| `Soundpost.Core.Engine` | classlib | AudioState, rules, profiles, diagnostics, reconcile. |
| `Soundpost.Core.Persistence` | classlib | Config store, backups, migrations, logging setup. |
| `Soundpost.App` | WPF app | UI, tray, MVVM view-models, composition root. |
| `Soundpost.Probe` | console | Headless harness to exercise `Core.Audio` without a UI. |
| `tests/*` | xUnit | Unit tests against services/fakes. |

## Testing strategy

- `Core.Engine`, `Core.Persistence`, `RulesEngine`, and `DiagnosticsEngine` are tested
  against **fake** `Core.Audio` services — no real audio hardware required in CI.
- `Core.Audio` itself is validated interactively via `Soundpost.Probe` on real machines,
  since it depends on live Windows audio state.
