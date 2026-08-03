using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Soundpost.App.Controls;

namespace Soundpost.App.Views;

public partial class VisualizerView : UserControl
{
    private Button? _activeStyle;

    public VisualizerView()
    {
        InitializeComponent();

        // Apply the initial knob positions, then react to drags.
        ApplySensitivity();
        ApplySmoothing();
        ApplyGlow();
        ApplySpeed();
        ApplyPalette();

        KnobSensitivity.ValueChanged += (_, _) => ApplySensitivity();
        KnobSmoothing.ValueChanged += (_, _) => ApplySmoothing();
        KnobGlow.ValueChanged += (_, _) => ApplyGlow();
        KnobSpeed.ValueChanged += (_, _) => ApplySpeed();
        KnobPalette.ValueChanged += (_, _) => ApplyPalette();

        SetActive(StyleRibbon, VizStyle.Ribbon);
    }

    private void ApplySensitivity()
    {
        Viz.Sensitivity = KnobSensitivity.Value;
        LblSensitivity.Text = Percent(KnobSensitivity.Value);
    }

    private void ApplySmoothing()
    {
        Viz.Smoothing = KnobSmoothing.Value;
        LblSmoothing.Text = Percent(KnobSmoothing.Value);
    }

    private void ApplyGlow()
    {
        Viz.GlowAmount = KnobGlow.Value;
        LblGlow.Text = Percent(KnobGlow.Value);
    }

    private void ApplySpeed()
    {
        Viz.Speed = KnobSpeed.Value;
        LblSpeed.Text = Percent(KnobSpeed.Value);
    }

    private void ApplyPalette()
    {
        int index = (int)Math.Round(KnobPalette.Value * (Viz.PaletteCount - 1));
        Viz.Palette = index;
        LblPalette.Text = Visualizer.PaletteNames[index];
    }

    private void OnStyle(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && Enum.TryParse((string)button.Tag, out VizStyle style))
        {
            SetActive(button, style);
        }
    }

    private void SetActive(Button button, VizStyle style)
    {
        Viz.VisualStyle = style;
        StyleTag.Text = button.Content?.ToString()?.ToUpperInvariant() ?? string.Empty;

        if (_activeStyle is not null)
        {
            _activeStyle.Style = (Style)FindResource("PillButton");
        }

        button.Style = (Style)FindResource("PillButtonActive");
        _activeStyle = button;
    }

    private static string Percent(double value) => (value * 100).ToString("0", CultureInfo.InvariantCulture) + "%";
}
