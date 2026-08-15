using System.Globalization;

namespace VetSucursales.Converters;

/// <summary>Devuelve la primera letra (en mayúscula) de un texto, para usarla como avatar.</summary>
public class InitialConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;
        return string.IsNullOrWhiteSpace(text) ? "?" : text.Trim()[..1].ToUpper(culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
