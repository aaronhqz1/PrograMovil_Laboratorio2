namespace VetSucursales.Models;

public class Sucursal
{
    public string? Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public Direccion Direccion { get; set; } = new();

    public string Telefono { get; set; } = string.Empty;

    public List<HorarioDia> Horario { get; set; } = CrearHorarioSemanaVacio();

    public Encargado Encargado { get; set; } = new();

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Los 7 días de la semana en orden, todos "Cerrado" por defecto.</summary>
    public static List<HorarioDia> CrearHorarioSemanaVacio() =>
        Enum.GetValues<DiaSemana>()
            .Select(dia => new HorarioDia { DiaSemana = dia, Estado = EstadoHorarioDia.Cerrado })
            .ToList();

    public Sucursal Clone() => new()
    {
        Id = Id,
        Nombre = Nombre,
        Direccion = Direccion.Clone(),
        Telefono = Telefono,
        Horario = Horario.Select(h => h.Clone()).ToList(),
        Encargado = Encargado.Clone(),
        Descripcion = Descripcion
    };
}
