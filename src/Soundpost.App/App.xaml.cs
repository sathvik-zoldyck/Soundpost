using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using H.NotifyIcon;
using Soundpost.App.ViewModels;
using Soundpost.App.Windows;
using Soundpost.Core.Audio;

namespace Soundpost.App;

/// <summary>
/// Composition root. For this first UI milestone the wiring is deliberately simple (manual
/// construction); it moves to Microsoft.Extensions.Hosting + Serilog in the persistence milestone.
///
/// Soundpost runs as a tray app: the window hides to the tray instead of exiting, a left-click on
/// the tray icon toggles the Quick Panel, and the app only quits from the tray menu.
/// </summary>
public partial class App : Application
{
    private const string InstanceName = "Soundpost.SingleInstance.v1";

    private Mutex? _singleInstance;
    private EventWaitHandle? _showSignal;

    private CoreAudioDeviceService? _deviceService;
    private CoreAudioSessionService? _sessionService;
    private CoreAudioMeterService? _meterService;
    private CoreAudioMasterVolumeService? _masterVolumeService;
    private MainViewModel? _mainViewModel;

    private MainWindow? _window;
    private QuickPanelWindow? _panel;
    private TaskbarIcon? _tray;
    private DateTime _panelHiddenAt;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single instance: a second launch signals the running one to surface, then bows out.
        _singleInstance = new Mutex(initiallyOwned: true, InstanceName, out bool isFirst);
        _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, InstanceName + ".Show");
        if (!isFirst)
        {
            _showSignal.Set();
            Shutdown();
            return;
        }

        WaitForShowSignals();

        DispatcherUnhandledException += OnUnhandledException;

        _deviceService = new CoreAudioDeviceService();
        _sessionService = new CoreAudioSessionService();
        _meterService = new CoreAudioMeterService();
        _masterVolumeService = new CoreAudioMasterVolumeService();
        IDefaultDeviceSwitcher switcher = new PolicyConfigDefaultDeviceSwitcher();

        _mainViewModel = new MainViewModel(
            _deviceService, _sessionService, switcher, _meterService, _masterVolumeService);

        _window = new MainWindow { DataContext = _mainViewModel };
        _window.CloseToTrayRequested += (_, _) => HideConsole();
        _window.StateChanged += (_, _) =>
        {
            if (_window.WindowState == WindowState.Minimized)
            {
                HideConsole();
            }
        };

        _panel = new QuickPanelWindow { DataContext = _mainViewModel };
        _panel.OpenConsoleRequested += (_, _) => ShowConsole();
        _panel.IsVisibleChanged += (_, args) =>
        {
            if (args.NewValue is false)
            {
                _panelHiddenAt = DateTime.UtcNow;
            }
        };

        CreateTrayIcon();

        _window.Show();
    }

    private void CreateTrayIcon()
    {
        _tray = new TaskbarIcon
        {
            ToolTipText = "Soundpost",
            // Hand it a real GDI icon. Setting IconSource to a PNG makes H.NotifyIcon try to read the
            // stream as an .ico and throw ("must be a picture that can be used as an Icon").
            Icon = LoadTrayIcon(),
            ContextMenu = BuildTrayMenu(),
        };

        // Left-click toggles the panel. The panel's own Deactivated hides it just before this fires
        // when it was already open, so a very recent hide means "toggle off" — don't reopen it.
        _tray.TrayLeftMouseUp += (_, _) =>
        {
            if ((DateTime.UtcNow - _panelHiddenAt).TotalMilliseconds > 250)
            {
                ShowPanel();
            }
        };

        _tray.ForceCreate();
    }

    // The full logo is far too detailed to survive a 16px tray slot — it turns to mud. So the tray
    // gets a purpose-built glyph: the brand's interlocking-S alone, stroked thick in the same
    // gradient, rendered crisp at 32px and left for the shell to downscale. GetHicon hands us an
    // unmanaged HICON that Icon.FromHandle wraps without owning; one handle lives for the whole
    // session and the OS reclaims it on exit.
    private static System.Drawing.Icon LoadTrayIcon()
    {
        const int size = 32;
        const double pad = 2.5;
        const double stroke = 58; // in the 512-unit logo space; deliberately heavy so it reads small

        var mark = Geometry.Parse(
            "M 336 176 C 296 148, 212 158, 212 202 C 212 236, 256 248, 256 256 " +
            "C 256 264, 300 276, 300 310 C 300 354, 216 364, 176 336");

        var gradient = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
        gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x7A, 0x1A), 0));
        gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x3D, 0x7F), 0.5));
        gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0xA2, 0x4B, 0xFF), 1));
        gradient.Freeze();

        var pen = new Pen(gradient, stroke)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
        };
        pen.Freeze();

        // Fit the stroked mark (centreline bounds grown by half the pen width) into the icon.
        Rect bounds = mark.Bounds;
        bounds.Inflate(stroke / 2, stroke / 2);
        double scale = (size - (2 * pad)) / Math.Max(bounds.Width, bounds.Height);
        double offsetX = (size - (bounds.Width * scale)) / 2;
        double offsetY = (size - (bounds.Height * scale)) / 2;

        var visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            dc.PushTransform(new TranslateTransform(offsetX, offsetY));
            dc.PushTransform(new ScaleTransform(scale, scale));
            dc.PushTransform(new TranslateTransform(-bounds.X, -bounds.Y));
            dc.DrawGeometry(null, pen, mark);
            dc.Pop();
            dc.Pop();
            dc.Pop();
        }

        var target = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);

        // Round-trip through PNG so alpha lands straight (not premultiplied) in the GDI bitmap.
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(target));
        using var buffer = new System.IO.MemoryStream();
        encoder.Save(buffer);
        buffer.Position = 0;

        using var bitmap = new System.Drawing.Bitmap(buffer);
        return System.Drawing.Icon.FromHandle(bitmap.GetHicon());
    }

    private ContextMenu BuildTrayMenu()
    {
        var menu = new ContextMenu();

        var open = new MenuItem { Header = "Open Soundpost" };
        open.Click += (_, _) => ShowConsole();

        var panel = new MenuItem { Header = "Quick Panel" };
        panel.Click += (_, _) => ShowPanel();

        var exit = new MenuItem { Header = "Exit" };
        exit.Click += (_, _) => ExitApp();

        menu.Items.Add(open);
        menu.Items.Add(panel);
        menu.Items.Add(new Separator());
        menu.Items.Add(exit);
        return menu;
    }

    private void ExitApp()
    {
        // Let the windows' OnClosing through, then tear down.
        if (_window is not null)
        {
            _window.AllowClose = true;
        }

        if (_panel is not null)
        {
            _panel.AllowClose = true;
        }

        Shutdown();
    }

    private void ShowPanel()
    {
        if (_panel is null)
        {
            return;
        }

        if (_panel.IsVisible)
        {
            _panel.Hide();
            return;
        }

        _panel.ShowNearTray();
    }

    private void ShowConsole()
    {
        if (_window is null)
        {
            return;
        }

        _panel?.Hide();
        if (_mainViewModel is not null)
        {
            _mainViewModel.MetersVisible = true;
        }

        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void HideConsole()
    {
        _window?.Hide();
        if (_mainViewModel is not null)
        {
            // Nothing metered is on screen while the console is tucked away — pause the peak poll.
            _mainViewModel.MetersVisible = false;
        }
    }

    /// <summary>Background wait so a second launch (or a re-run) pops the console on this instance.</summary>
    private void WaitForShowSignals()
    {
        var thread = new Thread(() =>
        {
            while (_showSignal!.WaitOne())
            {
                Dispatcher.BeginInvoke(ShowConsole);
            }
        })
        {
            IsBackground = true,
            Name = "Soundpost.ShowSignal",
        };
        thread.Start();
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.ToString(), "Soundpost — unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _mainViewModel?.Dispose();
        _masterVolumeService?.Dispose();
        _meterService?.Dispose();
        _sessionService?.Dispose();
        _deviceService?.Dispose();
        _showSignal?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
