using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Soundpost.App;

/// <summary>Bool → Visibility, inverted (true → Collapsed).</summary>
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility.Collapsed;
}

/// <summary>
/// Enum ⇆ bool against a ConverterParameter — lets the sidebar's RadioButtons bind straight to
/// <c>ActiveSection</c> (checked when the section matches, and navigating when clicked).
/// </summary>
public sealed class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() == parameter?.ToString();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Only the newly-checked button writes back; the one being unchecked must not clobber it.
        if (value is true && parameter is not null && targetType.IsEnum)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }

        return Binding.DoNothing;
    }
}

/// <summary>Returns the first letter of a string, uppercased — for the app tiles.</summary>
public sealed class FirstLetterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string s = value?.ToString()?.Trim() ?? string.Empty;
        return s.Length == 0 ? "?" : char.ToUpperInvariant(s[0]).ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Resolves an ambient sound's icon key (e.g. "Rain") to its <c>Geometry</c> resource
/// (<c>GlyphRain</c>), so a sound list can pick glyphs by name without a trigger per sound.
/// </summary>
public sealed class GlyphKeyConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string key = value?.ToString() ?? string.Empty;
        return Application.Current?.TryFindResource("Glyph" + key) as Geometry;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
