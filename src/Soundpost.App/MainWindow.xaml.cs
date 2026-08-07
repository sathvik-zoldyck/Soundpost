using System.ComponentModel;
using System.Windows;

namespace Soundpost.App;

public partial class MainWindow : Window
{
    /// <summary>Raised when the window is dismissed — the app hides to the tray instead of exiting.</summary>
    public event EventHandler? CloseToTrayRequested;

    /// <summary>Set true only for a real quit (tray → Exit), so the window is allowed to close.</summary>
    public bool AllowClose { get; set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    // Intercepts every close path — the title-bar button, Alt+F4, the system menu — so dismissing
    // the console tucks it into the tray. Only a real Exit (AllowClose) lets it through.
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            CloseToTrayRequested?.Invoke(this, EventArgs.Empty);
        }

        base.OnClosing(e);
    }
}
