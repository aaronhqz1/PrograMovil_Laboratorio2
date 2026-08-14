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

    /// <summary>Compara Estado y Rangos (no DiaSemana), para agrupar días con horario idéntico al editar.</summary>
    public bool TieneMismoHorarioQue(HorarioDia otro)
    {
        if (otro is null || Estado != otro.Estado)
            return false;

        if (Estado != EstadoHorarioDia.HorarioPersonalizado)
            return true;

        if (Rangos.Count != otro.Rangos.Count)
            return false;

        for (int i = 0; i < Rangos.Count; i++)
        {
            if (Rangos[i].HoraInicio != otro.Rangos[i].HoraInicio || Rangos[i].HoraCierre != otro.Rangos[i].HoraCierre)
                return false;
        }

        return true;
    }
}
