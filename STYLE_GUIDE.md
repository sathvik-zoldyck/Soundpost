# Style Guide

Two parts: how the **code** should read, and how the **design** should look. Both matter — Soundpost
is judged on reliability *and* on being a joy to use.

---

## Part 1 — Code

C#, .NET 9. Most rules are enforced by [`.editorconfig`](.editorconfig); run `dotnet format` before
every commit and CI's `dotnet format --verify-no-changes` will keep you honest.

### Conventions

- **File-scoped namespaces**, `using` directives outside the namespace.
- **Explicit types**, especially in interop and audio code — `MMDevice device = …`, not `var`, where
  the type isn't obvious. `var` is fine when the right-hand side makes the type plain.
- **Braces always**, even one-liners.
- **Nullable reference types on.** Don't paper over warnings with `!` unless you can justify it.
- **Naming:** `PascalCase` for types/methods/properties, `_camelCase` for private fields,
  `camelCase` for locals/params.
- Public APIs get XML doc comments. Keep them about *why*, not *what the code obviously does*.

### The rules that are non-negotiable

1. **The COM firewall.** Only `Soundpost.Core.Audio` may reference audio COM or NAudio. Everything
   else works against the normalized models and service interfaces. A PR that leaks COM upward will
   be sent back.
2. **Fail visibly, never silently.** The only place we swallow exceptions is a documented, transient
   COM failure (a session closing mid-enumeration, a device vanishing during a switch) — and it's
   always commented as such. Real errors surface in Diagnostics or the log. Never `catch {}` to make
   a warning go away.
3. **Actions are idempotent.** A command that sets state must be safe to run twice — the reconcile
   loop depends on it.
4. **No telemetry, no network calls.** If your feature needs the network, it needs an RFC and a very
   good reason.
5. **Interop is quarantined and commented.** Undocumented Windows interfaces get a comment saying
   what they do and which Windows versions they target.

### Async & threading

- Don't block the UI thread. Audio COM calls that may be slow belong off it.
- Meter/visualizer reads run on the render loop and must be allocation-light and lock-guarded.
- Marshal COM callbacks (`IMMNotificationClient`) to the UI thread before touching UI state.

### Commits

[Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `docs:`, `refactor:`,
`perf:`, `test:`, `chore:`. Scope optional: `feat(app):`, `fix(core-audio):`. Explain *why* in the body.

---

## Part 2 — Design

Soundpost looks like a **premium audio rack unit** — a dark console with tactile controls and
glowing meters. Restraint is the whole game: one accent, used as light, on deep neutral surfaces.

### Color tokens

| Token | Hex | Use |
|---|---|---|
| `bg` | `#0b0d11` | Deepest chassis background |
| `surface` | `#14171e` | Raised panels / cards |
| `surface-hi` | `#1a1e26` | Card top / highlight face |
| `inset` | `#080a0d` | Recessed screens, meter housings |
| `line` | `#232935` | Hairline borders |
| `text` | `#eef1f7` | Primary text |
| `text-2` | `#8b93a2` | Secondary text |
| `text-3` | `#586070` | Micro-labels, captions |
| **`accent`** | **`#ff8a3d`** | The one accent — active states, glow, faders. Use sparingly. |
| `accent-2` | `#ffb072` | Accent hover / highlight |
| `violet` | `#8b7bff` | Secondary, mostly for the visualizer/logo |

**Meter colors** (segment level): low `#37e0a0` → mid `#ffc24b` → hot `#ff5468`.

**Visualizer palettes** ship as named sets (Sunset, Aqua, Neon, Ember). New palettes are welcome.

> The accent is a *highlight, not a fill.* If a screen looks mostly orange, it's wrong. Amber marks
> what's active, live, or important — nothing else.

### Typography

| Role | Family | Notes |
|---|---|---|
| Labels / wordmark | **Bahnschrift** (fallback Segoe UI Semibold) | UPPERCASE, wide letter-spacing (`.2–.34em`) |
| Body / names | **Segoe UI** | Normal case |
| Numbers / readouts | **Cascadia Code** / Consolas | Percentages, dB, Hz |

All are Windows system fonts — no downloads, no CSP issues.

### Shape & depth

- Corner radius: cards `14–16px`, controls `7–11px`, chassis `26–28px`.
- Depth is subtle: a `1px` top highlight (`rgba(255,255,255,.05)`) + a soft drop shadow. Insets get an
  inner shadow. No heavy skeuomorphism — *flat with a hint of physical*, not photoreal knobs.
- Hairline separators, not boxes, to divide sections.

### Motion

- Motion serves the audio: meters and the visualizer react to real signal.
- Everything else is quiet — subtle hover states, no gratuitous animation. Respect
  `prefers-reduced-motion` where it applies.

### Voice (UI copy)

- Name things by what the user controls, in plain words. "Switch," not "Set default endpoint."
- Errors explain what happened and how to fix it, in the interface's voice — never vague, never an apology.
- An action keeps its name through the flow: the button that says **Apply** produces "Applied."

When in doubt, open the Visualizer, look at the Mixer, and match what's there.
