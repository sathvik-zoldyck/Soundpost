using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Soundpost.App;

/// <summary>Bool → Visibility, inverted (true → Collapsed).</summary>
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility.Collapsed;
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
