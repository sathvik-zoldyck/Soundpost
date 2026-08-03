# Plugins

Plugins react to Soundpost events (a device connects, a scene changes, audio peaks) and drive actions
— without touching the core. This is how the community automates things we never thought of.

Read the full contract, host API, and a complete example in [PLUGIN_SDK.md](../PLUGIN_SDK.md).

> **Status:** the plugin loader is in design (RFC stage) — the SDK doc is the target. This folder is
> where built-in and reference plugins will live, and where the loader will look for local ones. Want
> to help design it? Open an [RFC](../docs/rfcs/).

## The shape of a plugin

```csharp
public sealed class AutoDuckPlugin : SoundpostPlugin
{
    public override PluginInfo Info => new()
    {
        Id = "com.example.autoduck",
        Name = "Auto-Duck",
        Version = "1.0.0",
        Author = "you",
    };

    public override void OnAudioPeak(AudioPeak peak) { /* react */ }
}
```

## Safety

Plugins run in-process with full trust — only run ones you trust. See
[PLUGIN_SDK.md §7](../PLUGIN_SDK.md#7-trust--safety).
