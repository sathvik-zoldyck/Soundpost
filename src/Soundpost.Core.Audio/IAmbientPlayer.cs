namespace Soundpost.Core.Audio;

/// <summary>
/// Plays and mixes looping ambient sound layers (rain, fire, waves, …) — several at once, each with
/// its own volume. The UI owns which layers are on and how loud; this contract is the seam an audio
/// backend implements.
/// </summary>
/// <remarks>
/// Not yet implemented for real — see <c>NoopAmbientPlayer</c>. A production backend loops a
/// gapless CC-licensed clip per sound (e.g. via NAudio's <c>MixingSampleProvider</c> +
/// <c>LoopStream</c>) and blends the active layers to the default endpoint. Kept behind this
/// interface so the mixer UI and the audio engine can be built independently.
/// </remarks>
public interface IAmbientPlayer
{
    /// <summary>Start (or stop) a sound layer. <paramref name="soundId"/> is the sound's stable key.</summary>
    void SetActive(string soundId, bool active);

    /// <summary>Set a layer's mix level, 0–1. Takes effect immediately if the layer is playing.</summary>
    void SetVolume(string soundId, float volume);

    /// <summary>Stop every layer at once.</summary>
    void StopAll();
}
