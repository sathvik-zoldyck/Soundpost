# Contributing to Fader

Thanks for being here. Fader aims to be a welcoming, high-quality open-source project, and contributions of every size are valued — bug reports, docs, design ideas, and code alike.

## Ways to help

- 🐛 **Report a bug** — open an issue with your Windows version, what you did, and what happened.
- 💡 **Request a feature** — describe the *problem* you're hitting, not just the solution. Real pain points shape the roadmap.
- 🎨 **Design & UX** — mockups, flow critiques, and accessibility feedback are hugely useful.
- 🧑‍💻 **Code** — look for [`good first issue`](../../labels/good%20first%20issue) and [`help wanted`](../../labels/help%20wanted).

## Development setup

- Install the [.NET 9 SDK](https://dotnet.microsoft.com/download) on Windows 10 or 11.
- `dotnet build` from the repo root.
- `dotnet run --project tools/Fader.Probe` to exercise the audio layer headlessly (no UI needed).

## Project principles

These are the values the codebase is built around. PRs are reviewed against them:

1. **Reliability over convenience.** If a feature can leave a user's audio in a broken or confusing state, it needs a recovery path and clear diagnostics.
2. **Honesty about Windows limits.** We never pretend a workaround is magic. If something needs a virtual driver or a relaunch, we say so — in the UI.
3. **Simplicity survives.** Advanced features must not clutter the basic experience. Progressive disclosure, always.
4. **Everything is observable.** Actions are logged, failures are visible, and state is inspectable.
5. **Local-first, no accounts, no telemetry.** Full stop.

## Architecture at a glance

Only **`Fader.Core.Audio`** is allowed to touch Windows audio COM APIs. Everything above it works against normalized models and services. See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) before making cross-layer changes.

```
Core.Audio  ->  Core.Engine (state + rules + diagnostics)  ->  App (WPF/MVVM)
                        \-> Core.Persistence (config)
```

## Code style

- Enforced by [`.editorconfig`](.editorconfig). Run `dotnet format` before pushing.
- File-scoped namespaces, braces always, explicit types in interop code.
- Public APIs get XML doc comments. Interop with undocumented Windows interfaces gets a comment explaining *what* it does and *which Windows versions* it targets.

## Pull request workflow

1. Fork and branch from `main` (e.g. `feat/per-app-routing`, `fix/device-null-default`).
2. Keep PRs focused; one logical change per PR.
3. Use [Conventional Commits](https://www.conventionalcommits.org/) for titles: `feat:`, `fix:`, `docs:`, `refactor:`, `chore:`, `test:`.
4. Make sure `dotnet build` and `dotnet format --verify-no-changes` pass.
5. Describe *what* and *why*; link the issue.

## Commit sign-off & conduct

By contributing you agree your work is licensed under the project's [MIT License](LICENSE) and that you'll follow the [Code of Conduct](CODE_OF_CONDUCT.md).

Happy building. 🎛️
