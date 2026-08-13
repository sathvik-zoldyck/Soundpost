# Contributing to Soundpost

Thanks for being here. Soundpost aims to be a welcoming, high-quality open-source project, and
contributions of every size are valued — bug reports, docs, design, themes, visualizers, and code alike.

New here? Read the [Vision](VISION.md) first. It is short, and every decision flows from it.

## Ways to help

- **Report a bug** — [open an issue](../../issues/new/choose) with your Windows version, what you did, and what happened.
- **Request a feature** — describe the *problem*, not just the fix. Real pain shapes the [Roadmap](ROADMAP.md).
- **Design and UX** — mockups, critiques, and accessibility feedback are all useful.
- **Build a visualizer** — a render style for the "Sound, seen" view. The most fun way in.
- **Make a theme** — reskin the console.
- **Build a plugin** — automate something with the [Plugin SDK](PLUGIN_SDK.md).
- **Share your setup** — add a screenshot to the [Showcase](SHOWCASE.md).
- **Write code** — look for [`good first issue`](../../labels/good%20first%20issue) and [`help wanted`](../../labels/help%20wanted).

## Ground rules

Everything is judged against the [project principles](VISION.md#principles): local-first, no
telemetry, reliability over convenience, honesty about Windows, simplicity survives, open and extensible.
Be kind — see the [Code of Conduct](CODE_OF_CONDUCT.md).

## Development setup

- Install the [.NET 9 SDK](https://dotnet.microsoft.com/download) on Windows 10 or 11.
- `dotnet build` from the repo root.
- `dotnet run --project src/Soundpost.App` to launch the app.
- `dotnet run --project tools/Soundpost.Probe` to exercise the audio layer headlessly (no UI needed).
- `dotnet format` before pushing (CI verifies formatting).

## Repo structure

```
Soundpost/
├─ src/
│  ├─ Soundpost.Core.Audio/    the COM firewall — all Windows audio interop
│  ├─ Soundpost.Core.Storage/  crash-safe config store
│  └─ Soundpost.App/           the WPF console + visualizer
├─ tools/Soundpost.Probe/      headless audio harness
├─ docs/
│  ├─ rfcs/                     proposals for bigger changes
│  ├─ decisions/               architecture decision records (ADRs)
│  ├─ design/                  design docs
│  └─ showcase/                community setup screenshots
├─ visualizers/                community visualizer renderers
├─ themes/                     community themes
├─ plugins/                    community plugins
├─ examples/                   example plugins / configs
├─ assets/                     brand assets (logo, etc.)
└─ scripts/                    dev / build helpers
```

## How to contribute specific things

**A visualizer** — implement `IVisualizerRenderer` (see
[`src/Soundpost.App/Controls/Visualizers/`](src/Soundpost.App/Controls/Visualizers/) for the contract
and the built-in examples), register it in `Visualizer.cs`, and follow the
[design tokens](STYLE_GUIDE.md#part-2--design). Include a short GIF in your PR.

**A theme** — a palette dictionary of the color tokens, plus a swatch in Settings. See the
`Themes/Theme*.xaml` files for the shape.

**A plugin** — see [PLUGIN_SDK.md](PLUGIN_SDK.md). Start from the base class and keep it single-purpose.

**Docs and decisions** — small doc fixes: just PR them. Architectural changes: open an
[RFC](docs/rfcs/); record settled decisions as an [ADR](docs/decisions/).

**Your setup in the Showcase** — see [SHOWCASE.md](SHOWCASE.md).

## Coding standards

Follow the [Style Guide](STYLE_GUIDE.md). The short version: file-scoped namespaces, explicit types in
interop, the COM firewall stays intact, fail visibly, no telemetry.

## Pull request workflow

1. Fork and branch from `main` (for example `feat/cymatics-visualizer`, `fix/default-null-device`).
2. Keep PRs focused — one logical change.
3. Use [Conventional Commits](https://www.conventionalcommits.org/) for titles.
4. Make sure `dotnet build` and `dotnet format --verify-no-changes` pass.
5. Update docs and the [CHANGELOG](CHANGELOG.md) if behavior changed.
6. Describe *what* and *why*, and link the issue.

## License of contributions

Soundpost is licensed under the **GNU General Public License v3.0** ([LICENSE](LICENSE)). By submitting
a contribution, you agree it is licensed under GPLv3 and that you have the right to submit it. Copyleft
keeps Soundpost — and everything built on it — open for everyone.
