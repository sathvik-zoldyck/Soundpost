using System.Windows;
using System.Windows.Controls;

namespace Soundpost.App.Views;

/// <summary>Settings — currently the theme switcher. Reskins the whole console live.</summary>
public partial class SettingsView : UserControl
{
    private bool _ready;

    public SettingsView()
    {
        InitializeComponent();

        // Reflect the active theme without triggering a switch during construction.
        string current = (Application.Current as App)?.CurrentTheme ?? "Indigo";
        RadioButton active = current switch
        {
            "BlackRed" => ThemeBlackRed,
            "RichGold" => ThemeRichGold,
            "PinkBlossom" => ThemePinkBlossom,
            _ => ThemeIndigo,
        };
        active.IsChecked = true;

        _ready = true;
    }

    private void OnThemeChosen(object sender, RoutedEventArgs e)
    {
        // The initial IsChecked above fires this during construction; ignore until we're ready.
        if (!_ready || sender is not RadioButton { Tag: string theme })
        {
            return;
        }

        (Application.Current as App)?.ApplyTheme(theme);
    }
}
