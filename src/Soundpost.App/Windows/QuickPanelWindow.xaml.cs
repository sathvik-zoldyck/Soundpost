using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace Soundpost.App.Windows;

/// <summary>
/// The compact tray flyout: master volume, output switching and per-app mute/volume, nothing more.
/// Behaves like the Windows volume flyout — it opens near the tray, floats above other windows, and
/// dismisses the moment it loses focus. A single instance is reused; it hides rather than closes.
/// </summary>
public partial class QuickPanelWindow : Window
{
    /// <summary>Raised when the user asks for the full console (header icon or footer button).</summary>
    public event EventHandler? OpenConsoleRequested;

    /// <summary>Set true only during app shutdown, so the reusable panel can finally close.</summary>
    public bool AllowClose { get; set; }

    public QuickPanelWindow() => InitializeComponent();

    // The panel is a single reused instance — hide it on any close attempt so it can be shown
    // again, unless the app is genuinely exiting.
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }

    /// <summary>Position the panel against the tray corner and show it, focused.</summary>
    public void ShowNearTray()
    {
        // Show invisibly first so SizeToContent settles and ActualHeight is real before we place it.
        Opacity = 0;
        Show();
        UpdateLayout();

        PlaceNearTray();

        Opacity = 1;
        Activate();
        PlayEntrance();
    }

    // A short fade + rise from below the tray, matching the console's section-change motion.
    private void PlayEntrance()
    {
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        Root.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));
        RootShift.BeginAnimation(
            System.Windows.Media.TranslateTransform.YProperty,
            new DoubleAnimation(14, 0, TimeSpan.FromMilliseconds(190)) { EasingFunction = ease });
    }

    private void PlaceNearTray()
    {
        // WorkArea already excludes the taskbar, so the gap between it and the full screen tells us
        // which edge the taskbar (and therefore the tray) sits on. Anchor to the nearest corner.
        Rect work = SystemParameters.WorkArea;
        double screenW = SystemParameters.PrimaryScreenWidth;
        double screenH = SystemParameters.PrimaryScreenHeight;
        const double gap = 6;

        bool taskbarLeft = work.Left > 0;
        bool taskbarTop = work.Top > 0;

        // Horizontal: hug the left edge only when the taskbar is docked left; otherwise the right.
        Left = taskbarLeft ? work.Left + gap : work.Right - ActualWidth - gap;

        // Vertical: below the work area top for a top taskbar, otherwise above the work area bottom.
        Top = taskbarTop ? work.Top + gap : work.Bottom - ActualHeight - gap;

        // Never let the panel spill off the monitor if the app list makes it tall.
        Left = Math.Max(work.Left + gap, Math.Min(Left, screenW - ActualWidth - gap));
        Top = Math.Max(gap, Math.Min(Top, screenH - ActualHeight - gap));
    }

    private void OnDeactivated(object? sender, EventArgs e) => Hide();

    private void OnOpenConsole(object sender, RoutedEventArgs e)
    {
        Hide();
        OpenConsoleRequested?.Invoke(this, EventArgs.Empty);
    }
}
