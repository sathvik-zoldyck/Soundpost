# Vision

## What Soundpost is

**Soundpost is a mixing desk for your whole PC** — one open, local, beautiful place to control
where every sound goes, save your setups, automate the boring parts, and *see* your audio.

A soundpost is the small wooden dowel inside a violin that carries vibration from the strings to
the body. It's the quiet part that makes the whole instrument resonate. That's the job of this
app: the quiet layer that carries your audio to the right place, every time.

## The problem

Getting audio right on Windows means juggling three or four tools that don't talk to each other —
a volume mixer here, a device-switch hotkey there, a routing utility, and a lot of clicking in
Settings that **resets after every Windows update**. None of them remember your intent. You plug in
your headphones and nothing follows. Something breaks and Windows says nothing about why.

## The vision

One app that:

- **Switches devices instantly** and routes each app where it belongs.
- **Remembers your intent** — scenes for Music, Gaming, Movie Night, Meetings — and applies them
  automatically when your headphones connect, a game launches, or the clock says it's night.
- **Explains itself** — when audio is wrong, it tells you why, in plain language, with a one-click fix.
- **Is a joy to look at** — a premium console, live meters, and a music visualizer that turns
  whatever's playing into something worth watching.
- **Belongs to everyone** — open source, extensible by plugins, themes, and visualizers the
  community builds and shares.

## Principles

These are non-negotiable. Every feature and PR is judged against them.

1. **Local-first. No accounts. No telemetry.** Your audio setup is yours. Soundpost makes zero
   network calls and phones no one home. Ever.
2. **Reliability over convenience.** If a feature can leave your audio broken or confusing, it needs
   a recovery path and clear diagnostics — or it doesn't ship.
3. **Honesty about Windows.** We never dress a workaround up as magic. If something needs a virtual
   driver, a relaunch, or has latency, we say so — in the UI.
4. **Simplicity survives.** Advanced power must never clutter the basic experience. Progressive
   disclosure, always. A newcomer and a power user should both feel at home.
5. **Open and extensible.** The core stays small and well-documented so the community can build
   plugins, visualizers, and themes without touching it.
6. **Delight counts.** A tool people love looking at is a tool people keep, share, and star. Polish
   is a feature.

## Who it's for

People who own more than one way to hear their PC — speakers, headphones, a Bluetooth headset, an
HDMI TV, an audio interface — and are tired of Windows treating that as an afterthought. Gamers,
streamers, music lovers, remote workers, and tinkerers.

## Non-goals

Being clear about what Soundpost *won't* be keeps it focused:

- **Not a DAW or an EQ/DSP suite.** System-wide EQ is driver-level, fragile territory (that's
  [Equalizer APO](https://sourceforge.net/projects/equalizerapo/)'s job). We may *integrate* with
  such tools, but we won't reinvent them.
- **Not a cloud product.** No login, no sync server, no subscription.
- **Not bloatware.** No background updater empire, no bundled extras, no ads.

## The long game

The core does audio control brilliantly. Everything expressive lives in an **ecosystem**:

- **Plugins** react to events (a device connects, a scene changes, audio peaks) and automate things
  we never thought of — see [PLUGIN_SDK.md](PLUGIN_SDK.md).
- **Visualizers** are community-built render styles for the "Sound, seen" view.
- **Themes** reskin the console.
- **Showcase** is where people share their setups — see [SHOWCASE.md](SHOWCASE.md).

## North star

> When someone plugs in their headphones and the *right thing just happens* — and they smile at the
> waves dancing on screen — Soundpost is doing its job.

See the [Roadmap](ROADMAP.md) for how we get there, and [CONTRIBUTING.md](CONTRIBUTING.md) to help.
