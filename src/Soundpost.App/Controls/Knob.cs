using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Soundpost.App.Controls;

/// <summary>
/// A rotary knob with a 270° value arc, a raised face, and a pointer notch. Drag up/down to
/// change <see cref="Value"/> (0–1); it raises <see cref="ValueChanged"/> so a parent can apply it.
/// </summary>
public sealed class Knob : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(Knob),
        new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(
        nameof(Accent), typeof(Color), typeof(Knob),
        new FrameworkPropertyMetadata(Color.FromRgb(0xff, 0x8a, 0x3d), FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    public Color Accent { get => (Color)GetValue(AccentProperty); set => SetValue(AccentProperty, value); }

    /// <summary>Raised whenever <see cref="Value"/> changes (drag or programmatic).</summary>
    public event EventHandler? ValueChanged;

    private const double StartDeg = 135;
    private const double SweepDeg = 270;

    private double _dragStartY;
    private double _dragStartValue;

    private static readonly Pen TrackPen = FreezePen(Color.FromRgb(0x2b, 0x2b, 0x30), 5);

    public Knob()
    {
        Cursor = Cursors.SizeNS;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Knob)d).ValueChanged?.Invoke(d, EventArgs.Empty);

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        _dragStartY = e.GetPosition(this).Y;
        _dragStartValue = Value;
        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (IsMouseCaptured)
        {
            double delta = (_dragStartY - e.GetPosition(this).Y) / 160.0;
            Value = Math.Clamp(_dragStartValue + delta, 0, 1);
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        double size = Math.Min(ActualWidth, ActualHeight);
        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        double r = (size / 2) - 4;
        double value = Math.Clamp(Value, 0, 1);

        dc.DrawGeometry(null, TrackPen, Arc(center, r, StartDeg, SweepDeg));
        var accentPen = new Pen(new SolidColorBrush(Accent), 5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        dc.DrawGeometry(null, accentPen, Arc(center, r, StartDeg, SweepDeg * value));

        double fr = r - 6;
        var face = new RadialGradientBrush(Color.FromRgb(0x3a, 0x3a, 0x40), Color.FromRgb(0x16, 0x16, 0x1a))
        {
            GradientOrigin = new Point(0.4, 0.32),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.6,
            RadiusY = 0.6,
        };
        dc.DrawEllipse(face, FreezePen(Color.FromRgb(0x42, 0x42, 0x48), 1), center, fr, fr);

        double a = (StartDeg + (SweepDeg * value)) * Math.PI / 180;
        var outer = new Point(center.X + (Math.Cos(a) * (fr - 3)), center.Y + (Math.Sin(a) * (fr - 3)));
        var inner = new Point(center.X + (Math.Cos(a) * (fr - 12)), center.Y + (Math.Sin(a) * (fr - 12)));
        var notch = new Pen(new SolidColorBrush(Accent), 2.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        dc.DrawLine(notch, inner, outer);
    }

    private static Geometry Arc(Point c, double r, double startDeg, double sweepDeg)
    {
        double a0 = startDeg * Math.PI / 180;
        double a1 = (startDeg + sweepDeg) * Math.PI / 180;
        var p0 = new Point(c.X + (r * Math.Cos(a0)), c.Y + (r * Math.Sin(a0)));
        var p1 = new Point(c.X + (r * Math.Cos(a1)), c.Y + (r * Math.Sin(a1)));
        var fig = new PathFigure { StartPoint = p0, IsClosed = false };
        fig.Segments.Add(new ArcSegment(p1, new Size(r, r), 0, Math.Abs(sweepDeg) > 180, SweepDirection.Clockwise, true));
        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        geo.Freeze();
        return geo;
    }

    private static Pen FreezePen(Color color, double thickness)
    {
        var pen = new Pen(new SolidColorBrush(color), thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        pen.Freeze();
        return pen;
    }
}
