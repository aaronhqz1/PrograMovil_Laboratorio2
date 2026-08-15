namespace VetSucursales.Models;

public class HorarioDia
{
    public DiaSemana DiaSemana { get; set; }

    public EstadoHorarioDia Estado { get; set; } = EstadoHorarioDia.Cerrado;

    public List<RangoHorario> Rangos { get; set; } = new();

    public HorarioDia Clone() => new()
    {
        DiaSemana = DiaSemana,
        Estado = Estado,
        Rangos = Rangos.Select(r => r.Clone()).ToList()
    };
}
