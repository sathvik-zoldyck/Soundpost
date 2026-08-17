# Auto-Duck (reference plugin)

Lowers every other app while a voice-chat app is actually making sound, then restores the exact
original volumes. The classic "duck the music when a call starts" reflex — automated.

This is a **reference plugin**: the canonical example built against
[`Soundpost.Plugin.Abstractions`](../../src/Soundpost.Plugin.Abstractions/) and nothing else. It shows
the whole shape of a plugin in one small class — event handling, host actions, per-plugin storage, and
logging.

> **Status.** The plugin **loader** does not exist yet (it is [RFC 0001](../../docs/rfcs/0001-plugin-sdk.md),
> Phase 2), so this cannot be dropped into a running Soundpost *today*. It compiles against the real
> contract and is here to keep that contract honest and to be the starting point people copy. When the
> loader lands, the build output + `plugin.json` go in `%AppData%\Soundpost\plugins\autoduck\`.

## How it works

- A set of **comms apps** is matched by name (Discord, Teams, Zoom, …). When one of them peaks above a
  small activation level, a call is considered *live*.
- While live, every other app (except system sounds and the call itself) is set to the **duck level**;
  the original volume is remembered.
- A short **release window** holds the duck through brief gaps between words, so the volume doesn't
  pump. When it lapses, every ducked app is put back exactly where it was.
- On unload (disable / shutdown) it restores first, so nothing is left quiet.

## Configuration

Settings live in the plugin's own key/value storage (written with defaults on first run):

| Key | Default | Meaning |
|---|---|---|
| `comms.apps` | `Discord,Teams,Zoom,Slack,Webex,Meet,Skype,Mumble` | Comma-separated name fragments matched case-insensitively. |
| `duck.level` | `0.25` | Target volume (0–1) for ducked apps. Only ever lowers, never raises. |
| `duck.releaseMs` | `900` | How long the duck holds after the last comms sound, in milliseconds. |
| `comms.activationLevel` | `0.02` | Peak level (0–1) a comms app must exceed to count as "live". |

## The whole plugin

See [`AutoDuckPlugin.cs`](AutoDuckPlugin.cs). It derives from `SoundpostPlugin` (no-op defaults),
overrides only the handlers it needs, and never touches Windows audio COM — it reads and acts entirely
through `ISoundpostHost`.
