using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Soundpost.App.Controls.Visualizers;
using Soundpost.Core.Audio;

namespace Soundpost.App.Controls;

/// <summary>
/// The live visualizer host. Owns a <see cref="LoopbackAnalyzer"/> (captures only while visible),
/// smooths the FFT into bands each frame, and hands the frame to the selected
/// <see cref="IVisualizerRenderer"/>. Styles are a registry, not a switch — adding one is writing a
/// class and appending it to <see cref="_renderers"/>, which is exactly what a community style does.
/// </summary>
public sealed class Visualizer : FrameworkElement
{
    // The built-in styles, in the order they appear in the style bar. Community renderers append here.
    private readonly IVisualizerRenderer[] _renderers =
    {
        new RibbonRenderer(),
        new AuroraRenderer(),
        new SpectrumRenderer(),
        new RadialRenderer(),
        new OscilloscopeRenderer(),
        new CymaticsRenderer(),
        new CustomImageRenderer(),
    };

    /// <summary>The available styles, in display order.</summary>
    public IReadOnlyList<IVisualizerRenderer> Renderers => _renderers;

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex), typeof(int), typeof(Visualizer),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Index into <see cref="Renderers"/> of the style currently drawing.</summary>
    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }

    /// <summary>The style currently drawing.</summary>
    public IVisualizerRenderer SelectedRenderer => _renderers[Math.Clamp(SelectedIndex, 0, _renderers.Length - 1)];

    public double Sensitivity { get; set; } = 0.68;
    public double Smoothing { get; set; } = 0.55;
    public double GlowAmount { get; set; } = 0.72;
    public double Speed { get; set; } = 0.4;

    /// <summary>Image drawn by an <see cref="IRequiresImage"/> style; reacts to the audio.</summary>
    public ImageSource? CustomImage { get; set; }

    public static readonly string[] PaletteNames = { "Sunset", "Aqua", "Neon", "Ember" };

    private static readonly Color[][] Palettes =
    {
        new[] { Rgb(0xff, 0x7a, 0x1a), Rgb(0xff, 0x3d, 0x7f), Rgb(0xa2, 0x4b, 0xff) }, // Sunset
        new[] { Rgb(0x1a, 0xd1, 0xff), Rgb(0x3d, 0x7f, 0xff), Rgb(0x8b, 0x7b, 0xff) }, // Aqua
        new[] { Rgb(0x39, 0xff, 0x88), Rgb(0x2e, 0xe6, 0xff), Rgb(0xc2, 0x4b, 0xff) }, // Neon
        new[] { Rgb(0xff, 0xd0, 0x8a), Rgb(0xff, 0x8a, 0x3d), Rgb(0xc9, 0x53, 0x1c) }, // Ember
    };

    private const int Bands = 96;
    private readonly LoopbackAnalyzer _analyzer = new();
    private readonly float[] _bands = new float[Bands];
    private readonly float[] _wave = new float[600];
    private double _time;
    private bool _running;

    private int _palette;
    private int _paletteVersion;
    private VizPalette _vizPalette = null!;

    private int _frameParity;

    private readonly System.Diagnostics.Stopwatch _fpsClock = System.Diagnostics.Stopwatch.StartNew();
    private int _frames;

    /// <summary>Frames actually rendered in the last second — surfaced on the screen HUD.</summary>
    public int Fps { get; private set; }

    /// <summary>Raised about once a second when <see cref="Fps"/> changes.</summary>
    public event EventHandler? FpsUpdated;

    public int PaletteCount => Palettes.Length;

    public int Palette
    {
        get => _palette;
        set
        {
            _palette = Math.Clamp(value, 0, Palettes.Length - 1);
            _vizPalette = new VizPalette(++_paletteVersion, Palettes[_palette]);
            InvalidateVisual();
        }
    }

    public Visualizer()
    {
        _vizPalette = new VizPalette(++_paletteVersion, Palettes[_palette]);
        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible)
            {
                Start();
            }
            else
            {
                Stop();
            }
        };
        Unloaded += (_, _) => Stop();
    }

    private void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        _analyzer.Start();
        CompositionTarget.Rendering += OnFrame;
    }

    private void Stop()
    {
        if (!_running)
        {
            return;
        }

        _running = false;
        CompositionTarget.Rendering -= OnFrame;
        _analyzer.Stop();
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        _time += 0.016 * (0.35 + (Speed * 1.4));

        // The FFT runs at half the frame rate. Bands are heavily smoothed on the way out, so
        // a 30Hz refresh is indistinguishable from 60 while halving the transform cost.
        if ((++_frameParity & 1) == 0)
        {
            UpdateBands();
        }

        _analyzer.CopyWaveform(_wave);
        InvalidateVisual();

        _frames++;
        double elapsed = _fpsClock.Elapsed.TotalSeconds;
        if (elapsed >= 1.0)
        {
            Fps = (int)Math.Round(_frames / elapsed);
            _frames = 0;
            _fpsClock.Restart();
            FpsUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateBands()
    {
        float[] spec = _analyzer.GetSpectrum();
        int bins = spec.Length;
        double gain = 9 + (Sensitivity * 30);
        double smooth = 0.35 + (Smoothing * 0.6);

        for (int b = 0; b < Bands; b++)
        {
            double t0 = (double)b / Bands;
            double t1 = (double)(b + 1) / Bands;
            int lo = (int)(Math.Pow(t0, 2.2) * (bins - 1));
            int hi = Math.Max(lo + 1, (int)(Math.Pow(t1, 2.2) * (bins - 1)));

            float peak = 0f;
            for (int i = lo; i < hi && i < bins; i++)
            {
                if (spec[i] > peak)
                {
                    peak = spec[i];
                }
            }

            float val = (float)Math.Clamp(Math.Log10(1 + (peak * gain)) * 0.9, 0, 1);
            _bands[b] = (float)((_bands[b] * smooth) + (val * (1 - smooth)));
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w < 4 || h < 4)
        {
            return;
        }

        var frame = new VizFrame
        {
            Dc = dc,
            Width = w,
            Height = h,
            Bands = _bands,
            Waveform = _wave,
            Time = _time,
            Sensitivity = Sensitivity,
            Smoothing = Smoothing,
            Glow = GlowAmount,
            Speed = Speed,
            Palette = _vizPalette,
            Image = CustomImage,
        };

        _renderers[Math.Clamp(SelectedIndex, 0, _renderers.Length - 1)].Draw(frame);
    }

    private static Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b);
}
