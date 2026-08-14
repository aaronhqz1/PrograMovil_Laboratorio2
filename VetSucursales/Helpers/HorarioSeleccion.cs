using VetSucursales.Models;

namespace VetSucursales.Helpers;

/// <summary>Resultado que devuelve el modal de horario: el estado/rangos elegidos y a qué días
/// de la semana se les debe aplicar.</summary>
public class HorarioSeleccion
{
    public List<DiaSemana> Dias { get; init; } = new();

    public EstadoHorarioDia Estado { get; init; }

    public List<RangoHorario> Rangos { get; init; } = new();
}
