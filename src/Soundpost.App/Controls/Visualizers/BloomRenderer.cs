using System.Windows;
using System.Windows.Media;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// Rings that bloom from the centre on every bass onset and travel outward as they fade — a sonar
/// ping made of the beat. A small fixed pool of rings is advanced each frame (no per-frame
/// allocation); a slow running estimate of low-end energy decides when a ring is born, so the bloom
/// tracks the kick rather than firing on a timer. Alpha is baked into a few frozen tier pens instead
/// of a <c>PushOpacity</c> per ring, keeping the ring loop off the composition layers — the same
/// trick <see cref="RibbonRenderer"/> uses.
/// </summary>
public sealed class BloomRenderer : IVisualizerRenderer
{
    public string Name => "Bloom";

    private const int MaxRings = 24;

    // A parallel-array ring pool. A slot is free when its strength is 0, so there is nothing to
    // allocate or collect per frame.
    private readonly double[] _ringRadius = new double[MaxRings];
    private readonly double[] _ringStrength = new double[MaxRings]; // live 0..1; 0 = free slot
    private readonly double[] _ringBirth = new double[MaxRings];    // brightness the ring was born with

    private double _lastTime = double.NaN;
    private double _bassBaseline; // slow EMA of low-end energy — the onset floor
    private double _sinceSpawn;   // seconds since the last ring was born

    // Alpha tiers, pre-faded frozen pens: pick a tier by ring strength so the draw loop never calls
    // PushOpacity. Rebuilt only when the palette changes (tracked by Version).
    private int _penVersion = -1;
    private Pen[] _tierPens = System.Array.Empty<Pen>();
    private Pen _glowPen = null!;

    public void Draw(in VizFrame frame)
    {
        EnsurePens(frame.Palette);

        double dt = StepClock(frame.Time);
        double w = frame.Width, h = frame.Height;
        var centre = new Point(w / 2, h / 2);
        double maxRadius = Math.Min(w, h) * 0.5;

        // --- onset: rings are born from the low end, not a metronome ---
        int lowCount = Math.Max(1, frame.Bands.Length / 8);
        double bass = VizAudio.BandAvg(frame.Bands, 0, lowCount);
        _bassBaseline += (bass - _bassBaseline) * Math.Min(1, dt * 3.0); // ~1/3 s time constant
        _sinceSpawn += dt;

        double sensitivity = 0.5 + frame.Sensitivity; // the knob widens or narrows the whole response
        double threshold = (_bassBaseline * 1.35) + 0.02;
        if (bass > threshold && bass * sensitivity > 0.06 && _sinceSpawn > 0.09)
        {
            Spawn(Math.Min(1.0, bass * sensitivity));
            _sinceSpawn = 0;
        }

        // --- advance the pool, then draw ---
        double grow = maxRadius * 0.55 * dt; // px this frame; dt already carries the Speed knob
        DrawingContext dc = frame.Dc;
        int tiers = _tierPens.Length;

        // Bloom pass first, so the crisp rings sit on top of their own glow.
        if (frame.Glow > 0.05)
        {
            dc.PushOpacity(frame.Glow * 0.5);
            for (int i = 0; i < MaxRings; i++)
            {
                if (_ringStrength[i] > 0 && _ringRadius[i] > 4)
                {
                    dc.DrawEllipse(null, _glowPen, centre, _ringRadius[i], _ringRadius[i]);
                }
            }

            dc.Pop();
        }

        for (int i = 0; i < MaxRings; i++)
        {
            if (_ringStrength[i] <= 0)
            {
                continue;
            }

            _ringRadius[i] += grow;
            _ringStrength[i] = _ringBirth[i] * Math.Max(0, 1 - (_ringRadius[i] / maxRadius)); // fade with distance
            if (_ringStrength[i] <= 0)
            {
                continue; // reached the edge this frame — retire the slot
            }

            if (_ringRadius[i] > 2)
            {
                int tier = Math.Min(tiers - 1, (int)(_ringStrength[i] * tiers * 0.999));
                dc.DrawEllipse(null, _tierPens[tier], centre, _ringRadius[i], _ringRadius[i]);
            }
        }

        // A steady core so silence is never a blank frame: a small disc pulsing with overall energy.
        double energy = VizAudio.Energy(frame.Bands);
        double coreRadius = maxRadius * (0.03 + (energy * 0.06 * sensitivity));
        dc.DrawEllipse(frame.Palette.Gradient, null, centre, coreRadius, coreRadius);
    }

    private void Spawn(double strength)
    {
        for (int i = 0; i < MaxRings; i++)
        {
            if (_ringStrength[i] <= 0)
            {
                _ringRadius[i] = 0;
                _ringBirth[i] = strength;
                _ringStrength[i] = strength;
                return;
            }
        }

        // Pool full: replace the oldest (outermost) ring so new beats still register.
        int oldest = 0;
        for (int i = 1; i < MaxRings; i++)
        {
            if (_ringRadius[i] > _ringRadius[oldest])
            {
                oldest = i;
            }
        }

        _ringRadius[oldest] = 0;
        _ringBirth[oldest] = strength;
        _ringStrength[oldest] = strength;
    }

    // dt from the Speed-scaled clock, clamped so a reset or a long stall can't jump every ring to the
    // edge in a single frame.
    private double StepClock(double time)
    {
        if (double.IsNaN(_lastTime))
        {
            _lastTime = time;
            return 0;
        }

        double dt = time - _lastTime;
        _lastTime = time;
        return dt <= 0 ? 0 : Math.Min(dt, 0.1);
    }

    private void EnsurePens(VizPalette palette)
    {
        if (_penVersion == palette.Version)
        {
            return;
        }

        Brush gradient = VizBrush.HorizontalGradient(palette.Colors);
        _tierPens = new[]
        {
            VizBrush.FreezePen(VizBrush.Fade(gradient, 0.25), 1.2),
            VizBrush.FreezePen(VizBrush.Fade(gradient, 0.55), 1.6),
            VizBrush.FreezePen(VizBrush.Fade(gradient, 0.90), 2.0),
        };
        _glowPen = palette.GlowPen;
        _penVersion = palette.Version;
    }
}
