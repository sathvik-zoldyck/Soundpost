using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Soundpost.App.Controls;
using Soundpost.App.Controls.Visualizers;

namespace Soundpost.App.Windows;

/// <summary>
/// A fullscreen, always-on-top visualizer you can lay over a music video. The backdrop switches
/// between Solid (opaque), Dim (video shows through, muted) and Clear (video shows through fully);
/// the control bar auto-hides so the picture stays clean. Esc, or the Exit button, closes it.
/// </summary>
public partial class VisualizerOverlayWindow : Window
{
    private static readonly Brush DimBrush = Frozen(Color.FromArgb(0xC2, 0x06, 0x09, 0x11));
    private readonly Brush _solidBrush;
    private readonly DispatcherTimer _hideBar;
    private bool _ready;

    public VisualizerOverlayWindow()
    {
        InitializeComponent();
        _solidBrush = Backdrop.Background; // the radial gradient set in XAML

        // Auto-hide the control bar a couple of seconds after the mouse stops moving.
        _hideBar = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
        _hideBar.Tick += (_, _) =>
        {
            _hideBar.Stop();
            FadeBar(0);
            Mouse.OverrideCursor = Cursors.None;
        };

        // The style ListBox raises SelectionChanged while the XAML is still loading; ignore events
        // until construction has finished wiring up the timer and brushes.
        _ready = true;
    }

    /// <summary>Open the overlay continuing the style, palette and knob values from the console.</summary>
    public void ContinueFrom(Visualizer source)
    {
        Viz.SelectedIndex = source.SelectedIndex;
        Viz.Palette = source.Palette;
        Viz.Sensitivity = source.Sensitivity;
        Viz.Smoothing = source.Smoothing;
        Viz.GlowAmount = source.GlowAmount;
        Viz.Speed = source.Speed;
        Viz.CustomImage = source.CustomImage;
        UpdateImageHint();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Cover the whole primary screen (over the taskbar too), so it truly sits on top of a video.
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;

        Activate();
        Focus();
        BumpBar();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnStyleChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!_ready)
        {
            return;
        }

        UpdateImageHint();
        BumpBar();
    }

    private void OnModeChanged(object sender, RoutedEventArgs e)
    {
        // Solid: opaque backdrop. Dim: translucent scrim (video shows, muted). Clear: fully see-through.
        // The initial IsChecked fires this during InitializeComponent, before the sibling radio
        // buttons and the ctor's brushes exist — skip until construction has finished.
        if (!_ready)
        {
            return;
        }

        if (ModeDim.IsChecked == true)
        {
            Backdrop.Background = DimBrush;
        }
        else if (ModeClear.IsChecked == true)
        {
            Backdrop.Background = Brushes.Transparent;
        }
        else
        {
            Backdrop.Background = _solidBrush;
        }

        BumpBar();
    }

    private void UpdateImageHint() =>
        ImageHint.Visibility = Viz.SelectedRenderer is IRequiresImage && Viz.CustomImage is null
            ? Visibility.Visible
            : Visibility.Collapsed;

    // ---- auto-hiding control bar ----

    private void OnMouseMove(object sender, MouseEventArgs e) => BumpBar();

    private void BumpBar()
    {
        if (!_ready)
        {
            return;
        }

        Mouse.OverrideCursor = null;
        FadeBar(1);
        _hideBar.Stop();
        _hideBar.Start();
    }

    private void FadeBar(double to) =>
        Bar.BeginAnimation(OpacityProperty, new DoubleAnimation(to, TimeSpan.FromMilliseconds(200)));

    protected override void OnClosed(EventArgs e)
    {
        _hideBar.Stop();
        Mouse.OverrideCursor = null;
        base.OnClosed(e);
    }

    private static Brush Frozen(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }
}
