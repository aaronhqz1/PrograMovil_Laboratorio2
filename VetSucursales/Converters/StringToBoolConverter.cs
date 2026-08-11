using System.Globalization;

namespace VetSucursales.Converters;

/// <summary>Devuelve true cuando el string vinculado no está vacío (útil para mostrar mensajes de error).</summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
