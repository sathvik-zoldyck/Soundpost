using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Soundpost.App.Controls;

namespace Soundpost.App.Views;

public partial class VisualizerView : UserControl
{
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

        SetActive(VizStyle.Ribbon);
    }

    private void OnStyle(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && Enum.TryParse((string)button.Tag, out VizStyle style))
        {
            SetActive(style);
        }
    }

    private void OnToggleMore(object sender, RoutedEventArgs e) => MorePopup.IsOpen = !MorePopup.IsOpen;

    private void OnChooseImage(object sender, RoutedEventArgs e) => ChooseImage();

    private void SetActive(VizStyle style)
    {
        Viz.VisualStyle = style;
        StyleTag.Text = Display(style).ToUpperInvariant();

        StyleRibbon.Style = Pill(style == VizStyle.Ribbon);
        StyleSpectrum.Style = Pill(style == VizStyle.Spectrum);
        StyleRadial.Style = Pill(style == VizStyle.Radial);
        StyleScope.Style = Pill(style == VizStyle.Oscilloscope);

        bool isMore = style is VizStyle.Cymatics or VizStyle.CustomImage;
        MoreButton.Style = Pill(isMore);
        MoreButton.Content = isMore ? Display(style) + "  ▾" : "More  ▾";

        MorePopup.IsOpen = false;
        UpdateImageOverlay();
    }

    // Keeps the Custom Image chrome in sync: drop-zone when empty, "Change image…" pill once loaded.
    private void UpdateImageOverlay()
    {
        bool custom = Viz.VisualStyle == VizStyle.CustomImage;
        bool hasImage = Viz.CustomImage is not null;

        ImageEmptyState.Visibility = custom && !hasImage ? Visibility.Visible : Visibility.Collapsed;
        ImageButton.Visibility = custom && hasImage ? Visibility.Visible : Visibility.Collapsed;
        ImageButton.Content = "Change image…";
    }

    private void ChooseImage()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose an image to visualize",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
        };

        // Parent to the window so the dialog can never open behind the custom chrome.
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
        {
            LoadImage(dialog.FileName);
        }
    }

    private bool LoadImage(string path)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // load now so we don't lock the file
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            bitmap.Freeze();
            Viz.CustomImage = bitmap;
            UpdateImageOverlay();
            return true;
        }
        catch
        {
            // Unsupported or corrupt image — leave the previous one (or the drop-zone) in place.
            return false;
        }
    }

    // ---- Drag & drop: drop an image anywhere on the screen to load it ----

    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    private void OnDragEnter(object sender, DragEventArgs e) => OnDragOver(sender, e);

    private void OnDragOver(object sender, DragEventArgs e)
    {
        string? path = ImagePathFrom(e);
        if (path is not null)
        {
            e.Effects = DragDropEffects.Copy;
            DragFileName.Text = Path.GetFileName(path);
            DragOverlay.Visibility = Visibility.Visible;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        // DragLeave also fires when crossing onto child elements, so only hide once the
        // pointer is truly outside the screen — otherwise the overlay flickers.
        Point p = e.GetPosition(ScreenBorder);
        if (p.X < 0 || p.Y < 0 || p.X > ScreenBorder.ActualWidth || p.Y > ScreenBorder.ActualHeight)
        {
            DragOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        DragOverlay.Visibility = Visibility.Collapsed;

        string? path = ImagePathFrom(e);
        if (path is not null && LoadImage(path))
        {
            SetActive(VizStyle.CustomImage); // drop from any style jumps straight into Custom Image
        }
    }

    private static string? ImagePathFrom(DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
        {
            string ext = Path.GetExtension(files[0]).ToLowerInvariant();
            if (Array.IndexOf(ImageExtensions, ext) >= 0)
            {
                return files[0];
            }
        }

        return null;
    }

    private Style Pill(bool active) => (Style)FindResource(active ? "PillButtonActive" : "PillButton");

    private static string Display(VizStyle style) => style switch
    {
        VizStyle.CustomImage => "Custom Image",
        _ => style.ToString(),
    };

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

    private static string Percent(double value) => (value * 100).ToString("0", CultureInfo.InvariantCulture) + "%";
}
