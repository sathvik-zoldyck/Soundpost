using System.Windows;
using System.Windows.Threading;
using Soundpost.App.ViewModels;
using Soundpost.Core.Audio;

namespace Soundpost.App;

/// <summary>
/// Composition root. For this first UI milestone the wiring is deliberately simple (manual
/// construction); it moves to Microsoft.Extensions.Hosting + Serilog in the persistence milestone.
/// </summary>
public partial class App : Application
{
    private CoreAudioDeviceService? _deviceService;
    private CoreAudioSessionService? _sessionService;
    private CoreAudioMeterService? _meterService;
    private CoreAudioMasterVolumeService? _masterVolumeService;
    private MainViewModel? _mainViewModel;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandledException;

        _deviceService = new CoreAudioDeviceService();
        _sessionService = new CoreAudioSessionService();
        _meterService = new CoreAudioMeterService();
        _masterVolumeService = new CoreAudioMasterVolumeService();
        IDefaultDeviceSwitcher switcher = new PolicyConfigDefaultDeviceSwitcher();

        _mainViewModel = new MainViewModel(
            _deviceService, _sessionService, switcher, _meterService, _masterVolumeService);

        var window = new MainWindow { DataContext = _mainViewModel };
        window.Show();
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.ToString(), "Soundpost — unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mainViewModel?.Dispose();
        _masterVolumeService?.Dispose();
        _meterService?.Dispose();
        _sessionService?.Dispose();
        _deviceService?.Dispose();
        base.OnExit(e);
    }
}
