using System.Globalization;
using VetSucursales.Helpers;
using VetSucursales.Models;

namespace VetSucursales.Converters;

/// <summary>Formatea un objeto Direccion en una sola línea legible (listado y detalle).</summary>
public class DireccionFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Direccion direccion ? DisplayFormatter.FormatearDireccion(direccion) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
