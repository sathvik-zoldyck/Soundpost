using Soundpost.Core.Audio;

namespace Soundpost.App.Audio;

/// <summary>
/// Placeholder <see cref="IAmbientPlayer"/> — the mixer UI drives it, but it produces no sound yet.
/// The real looping/mixing engine (and the bundled CC-licensed clips) is a tracked contribution;
/// see the "Ambient soundscapes" issue. Swapping this out for a real backend needs no UI changes.
/// </summary>
public sealed class NoopAmbientPlayer : IAmbientPlayer
{
    public void SetActive(string soundId, bool active)
    {
        // no-op until a real audio backend lands
    }

    public void SetVolume(string soundId, float volume)
    {
        // no-op until a real audio backend lands
    }

    public void StopAll()
    {
        // no-op until a real audio backend lands
    }
}
