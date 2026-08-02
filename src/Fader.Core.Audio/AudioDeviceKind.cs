namespace Fader.Core.Audio;

/// <summary>Direction of an audio endpoint.</summary>
public enum AudioDeviceKind
{
    /// <summary>Output / render endpoint (speakers, headphones, HDMI).</summary>
    Playback,

    /// <summary>Input / capture endpoint (microphones, line-in).</summary>
    Recording,
}
