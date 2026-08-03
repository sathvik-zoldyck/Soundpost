using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;

namespace Soundpost.Core.Audio;

/// <summary>
/// Captures whatever is playing on the default output device (WASAPI loopback) and exposes a
/// recent mono waveform plus an FFT magnitude spectrum. This is what the Visualizer draws.
/// Capture runs on NAudio's own thread; reads are lock-guarded and allocation-free.
/// </summary>
public sealed class LoopbackAnalyzer : IDisposable
{
    private const int FftSize = 1024;   // must be 2^FftPow
    private const int FftPow = 10;
    private const int RingSize = 4096;

    private readonly object _lock = new();
    private readonly float[] _ring = new float[RingSize];
    private readonly Complex[] _fft = new Complex[FftSize];
    private readonly float[] _spectrum = new float[FftSize / 2];

    private WasapiLoopbackCapture? _capture;
    private int _writePos;
    private int _channels = 2;

    public bool IsRunning { get; private set; }

    public int SpectrumBins => FftSize / 2;

    public void Start()
    {
        Stop();
        try
        {
            _capture = new WasapiLoopbackCapture();
            _channels = Math.Max(1, _capture.WaveFormat.Channels);
            _capture.DataAvailable += OnDataAvailable;
            _capture.StartRecording();
            IsRunning = true;
        }
        catch
        {
            IsRunning = false;
            _capture = null;
        }
    }

    public void Stop()
    {
        WasapiLoopbackCapture? capture = _capture;
        _capture = null;
        IsRunning = false;
        if (capture is null)
        {
            return;
        }

        try
        {
            capture.DataAvailable -= OnDataAvailable;
            capture.StopRecording();
            capture.Dispose();
        }
        catch
        {
            // Already torn down.
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        // Loopback mix format is 32-bit IEEE float, interleaved per channel.
        int stride = 4 * _channels;
        int frames = e.BytesRecorded / stride;
        lock (_lock)
        {
            for (int i = 0; i < frames; i++)
            {
                float sum = 0f;
                int baseIdx = i * stride;
                for (int c = 0; c < _channels; c++)
                {
                    sum += BitConverter.ToSingle(e.Buffer, baseIdx + (c * 4));
                }

                _ring[_writePos] = sum / _channels;
                _writePos = (_writePos + 1) % RingSize;
            }
        }
    }

    /// <summary>Fills <paramref name="dest"/> with the most recent samples (‑1..1).</summary>
    public void CopyWaveform(float[] dest)
    {
        lock (_lock)
        {
            int n = dest.Length;
            for (int i = 0; i < n; i++)
            {
                int src = ((_writePos - n + i) % RingSize + RingSize) % RingSize;
                dest[i] = _ring[src];
            }
        }
    }

    /// <summary>Returns the current FFT magnitude spectrum (index 0 = low frequency).</summary>
    public float[] GetSpectrum()
    {
        lock (_lock)
        {
            for (int i = 0; i < FftSize; i++)
            {
                int src = ((_writePos - FftSize + i) % RingSize + RingSize) % RingSize;
                float window = (float)FastFourierTransform.HammingWindow(i, FftSize);
                _fft[i].X = _ring[src] * window;
                _fft[i].Y = 0f;
            }
        }

        FastFourierTransform.FFT(true, FftPow, _fft);
        for (int i = 0; i < _spectrum.Length; i++)
        {
            _spectrum[i] = MathF.Sqrt((_fft[i].X * _fft[i].X) + (_fft[i].Y * _fft[i].Y));
        }

        return _spectrum;
    }

    public void Dispose() => Stop();
}
