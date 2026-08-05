using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Soundpost.App.Controls;

/// <summary>
/// The big master-volume dial: a 270° track with a gradient fill, a draggable thumb, and a gap at
/// the bottom. Drag anywhere on the dial (or use the wheel / arrow keys) to set <see cref="Value"/>.
/// The centre readout is layered on top in XAML so it picks up the app's fonts.
/// </summary>
public sealed class ArcGauge : FrameworkElement
{
    private const double StartAngle = -135;  // 0° is 12 o'clock, clockwise positive
    private const double SweepAngle = 270;
    private const double Thickness = 16;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(ArcGauge),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null,
            CoerceValue));

    public static readonly DependencyProperty IsMutedProperty = DependencyProperty.Register(
        nameof(IsMuted), typeof(bool), typeof(ArcGauge),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Dial position, 0–1.</summary>
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    /// <summary>Dims the dial to show the endpoint is muted.</summary>
    public bool IsMuted { get => (bool)GetValue(IsMutedProperty); set => SetValue(IsMutedProperty, value); }

    private static object CoerceValue(DependencyObject d, object baseValue) =>
        Math.Clamp((double)baseValue, 0, 1);

    private static readonly Brush TrackBrush = Frozen(Color.FromRgb(0x1A, 0x1E, 0x26));
    private static readonly Brush ThumbBrush = Frozen(Color.FromRgb(0xF4, 0xF6, 0xFA));
    private static readonly Brush ThumbRing = Frozen(Color.FromArgb(0x66, 0x00, 0x00, 0x00));

    private static SolidColorBrush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private Brush? _well;
    private Pen? _wellRim;

    private static Brush BuildWell()
    {
        // Domed face lit from the upper left, like a machined knob cap.
        var brush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.36, 0.26),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.78,
            RadiusY = 0.78,
        };
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x18, 0x20, 0x3A), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x0C, 0x11, 0x20), 0.7));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x06, 0x09, 0x11), 1));
        brush.Freeze();
        return brush;
    }

    private static Pen BuildWellRim()
    {
        var pen = new Pen(Frozen(Color.FromRgb(0x1E, 0x27, 0x40)), 1);
        pen.Freeze();
        return pen;
    }

    private readonly Pen _trackPen;
    private readonly Pen _fillPen;
    private readonly Pen _glowPen;

    public ArcGauge()
    {
        Focusable = true;
        Cursor = Cursors.Hand;

        _trackPen = new Pen(TrackBrush, Thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        _trackPen.Freeze();

        // Same orange → pink → violet run as the Soundpost mark.
        var gradient = new LinearGradientBrush { StartPoint = new Point(0, 1), EndPoint = new Point(1, 0) };
        gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x7A, 0x1A), 0));
        gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x3D, 0x7F), 0.5));
        gradient.GradientStops.Add(new GradientStop(0xA24BFFu.ToColor(), 1));
        gradient.Freeze();

        _fillPen = new Pen(gradient, Thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        _fillPen.Freeze();

        var glow = gradient.Clone();
        glow.Opacity = 0.22;
        glow.Freeze();
        _glowPen = new Pen(glow, Thickness + 12) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        _glowPen.Freeze();
    }

    // A bare FrameworkElement has no natural size, so centred alignment would collapse it to zero.
    // Claim the largest square the parent offers, falling back to a sane default when unconstrained.
    protected override Size MeasureOverride(Size availableSize)
    {
        double side = Math.Min(availableSize.Width, availableSize.Height);
        if (double.IsInfinity(side) || double.IsNaN(side) || side <= 0)
        {
            side = 200;
        }

        return new Size(side, side);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        double radius = (Math.Min(w, h) / 2) - (Thickness / 2) - 8;
        if (radius <= 4)
        {
            return;
        }

        var centre = new Point(w / 2, h / 2);

        // Hit surface — without a filled background the element ignores clicks on empty pixels.
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, w, h));

        // Recessed well: darker at the top where the rim would shade it, so the dial reads as
        // sunk into the panel rather than floating on it.
        double wellRadius = radius + (Thickness / 2) + 5;
        dc.DrawEllipse(_well ??= BuildWell(), _wellRim ??= BuildWellRim(), centre, wellRadius, wellRadius);

        dc.DrawGeometry(null, _trackPen, Arc(centre, radius, StartAngle, StartAngle + SweepAngle));

        double value = Math.Clamp(Value, 0, 1);
        if (value > 0.001)
        {
            double end = StartAngle + (SweepAngle * value);
            Geometry fill = Arc(centre, radius, StartAngle, end);

            dc.PushOpacity(IsMuted ? 0.35 : 1.0);
            dc.DrawGeometry(null, _glowPen, fill);
            dc.DrawGeometry(null, _fillPen, fill);

            Point thumb = PointOnArc(centre, radius, end);
            dc.DrawEllipse(ThumbBrush, null, thumb, Thickness / 2, Thickness / 2);
            dc.DrawEllipse(null, new Pen(ThumbRing, 1), thumb, (Thickness / 2) + 0.5, (Thickness / 2) + 0.5);
            dc.Pop();
        }
    }

    private static Geometry Arc(Point centre, double radius, double fromDeg, double toDeg)
    {
        var geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(PointOnArc(centre, radius, fromDeg), false, false);
            ctx.ArcTo(
                PointOnArc(centre, radius, toDeg),
                new Size(radius, radius),
                0,
                toDeg - fromDeg > 180,
                SweepDirection.Clockwise,
                true,
                false);
        }

        geometry.Freeze();
        return geometry;
    }

    private static Point PointOnArc(Point centre, double radius, double degrees)
    {
        double rad = degrees * Math.PI / 180;
        return new Point(centre.X + (radius * Math.Sin(rad)), centre.Y - (radius * Math.Cos(rad)));
    }

    // ---- input ----

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        CaptureMouse();
        Focus();
        SetFromPoint(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (IsMouseCaptured)
        {
            SetFromPoint(e.GetPosition(this));
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        Value += e.Delta > 0 ? 0.02 : -0.02;
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left or Key.Down: Value -= 0.02; e.Handled = true; break;
            case Key.Right or Key.Up: Value += 0.02; e.Handled = true; break;
        }
    }

    private void SetFromPoint(Point p)
    {
        double dx = p.X - (ActualWidth / 2);
        double dy = p.Y - (ActualHeight / 2);
        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
        {
            return;
        }

        // Angle measured from 12 o'clock, clockwise, in -180..180.
        double degrees = Math.Atan2(dx, -dy) * 180 / Math.PI;

        // Snap the dead zone at the bottom to whichever end the pointer is nearest.
        const double end = StartAngle + SweepAngle;
        if (degrees < StartAngle)
        {
            Value = 0;
        }
        else if (degrees > end)
        {
            Value = 1;
        }
        else
        {
            Value = (degrees - StartAngle) / SweepAngle;
        }
    }
}

internal static class ColorExtensions
{
    /// <summary>0xRRGGBB → opaque Color, so palette constants read like their hex codes.</summary>
    public static Color ToColor(this uint rgb) =>
        Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
}
