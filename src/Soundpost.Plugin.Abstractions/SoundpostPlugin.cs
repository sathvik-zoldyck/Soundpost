using Soundpost.Core.Audio;

namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// Base class for plugins: every event is a no-op by default, so a plugin overrides only what it needs.
/// <see cref="Host"/> is set for you before any event fires. You must still supply <see cref="Info"/>.
/// </summary>
public abstract class SoundpostPlugin : ISoundpostPlugin
{
    /// <inheritdoc />
    public abstract PluginInfo Info { get; }

    /// <summary>The host, available from <see cref="OnLoaded"/> onward.</summary>
    protected ISoundpostHost Host { get; private set; } = null!;

    /// <inheritdoc />
    public virtual void OnLoaded(ISoundpostHost host) => Host = host;

    /// <inheritdoc />
    public virtual void OnUnloaded()
    {
    }

    /// <inheritdoc />
    public virtual void OnDeviceConnected(AudioDevice device)
    {
    }

    /// <inheritdoc />
    public virtual void OnDeviceDisconnected(AudioDevice device)
    {
    }

    /// <inheritdoc />
    public virtual void OnDefaultDeviceChanged(AudioDevice device, DeviceRole role)
    {
    }

    /// <inheritdoc />
    public virtual void OnSceneChanged(Scene scene)
    {
    }

    /// <inheritdoc />
    public virtual void OnAppStarted(AudioSession session)
    {
    }

    /// <inheritdoc />
    public virtual void OnAppClosed(int processId)
    {
    }

    /// <inheritdoc />
    public virtual void OnAudioPeak(AudioPeak peak)
    {
    }
}
