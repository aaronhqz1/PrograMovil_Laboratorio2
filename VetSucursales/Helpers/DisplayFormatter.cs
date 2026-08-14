using VetSucursales.Models;

namespace VetSucursales.Helpers;

/// <summary>Formatea los objetos estructurados (Direccion, HorarioDia) como texto legible para
/// mostrarlos en el listado, el detalle y el resumen compacto del formulario.</summary>
public static class DisplayFormatter
{
    private static readonly string[] NombresDias =
    {
        "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
    };

    private static readonly string[] InicialesDias = { "L", "M", "M", "J", "V", "S", "D" };

    public static string NombreDia(DiaSemana dia) => NombresDias[(int)dia];

    public static string InicialDia(DiaSemana dia) => InicialesDias[(int)dia];

    /// <summary>Ej. "11:00 a.m." — formato de 12 horas en minúsculas, como en la referencia de diseño.</summary>
    public static string FormatearHora(TimeSpan hora)
    {
        int hora12 = hora.Hours % 12;
        if (hora12 == 0)
            hora12 = 12;

        string ampm = hora.Hours < 12 ? "a.m." : "p.m.";
        return $"{hora12}:{hora.Minutes:D2} {ampm}";
    }

    /// <summary>Ej. "11:00 a.m. - 10:30 p.m." o "Cerrado" o "Abierto las 24 horas".</summary>
    public static string FormatearHorarioDia(HorarioDia dia) => dia.Estado switch
    {
        EstadoHorarioDia.Cerrado => "Cerrado",
        EstadoHorarioDia.Abierto24h => "Abierto las 24 horas",
        _ => dia.Rangos.Count == 0
            ? "Sin horario configurado"
            : string.Join(", ", dia.Rangos.Select(r => $"{FormatearHora(r.HoraInicio)} - {FormatearHora(r.HoraCierre)}"))
    };

    /// <summary>Dirección completa en una sola línea, omitiendo las partes vacías.</summary>
    public static string FormatearDireccion(Direccion direccion)
    {
        var partes = new[]
        {
            direccion.Direccion1,
            direccion.Direccion2,
            direccion.Ciudad,
            direccion.Estado,
            direccion.CodigoPostal
        };

        var texto = string.Join(", ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));
        return string.IsNullOrWhiteSpace(texto) ? "Sin dirección registrada" : texto;
    }
}
