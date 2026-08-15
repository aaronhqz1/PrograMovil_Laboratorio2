using VetSucursales.Models;

namespace VetSucursales.Helpers;

/// <summary>Fila de solo lectura para mostrar un día y su horario formateado (resumen del
/// formulario y bloque de horario del detalle). Se reconstruye cada vez que cambia el horario.</summary>
public class HorarioResumenItem
{
    public required HorarioDia Dia { get; init; }

    public string NombreDia => DisplayFormatter.NombreDia(Dia.DiaSemana);

    public string Descripcion => DisplayFormatter.FormatearHorarioDia(Dia);

    public static List<HorarioResumenItem> DesdeHorario(IEnumerable<HorarioDia> horario) =>
        horario
            .OrderBy(h => (int)h.DiaSemana)
            .Select(h => new HorarioResumenItem { Dia = h })
            .ToList();
}
