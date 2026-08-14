namespace VetSucursales.Models;

public class RangoHorario
{
    public TimeSpan HoraInicio { get; set; }

    public TimeSpan HoraCierre { get; set; }

    public RangoHorario Clone() => new()
    {
        HoraInicio = HoraInicio,
        HoraCierre = HoraCierre
    };
}
