namespace VetSucursales.Models;

public class Sucursal
{
    public string? Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string HorarioAtencion { get; set; } = string.Empty;

    public string Encargado { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public Sucursal Clone() => new()
    {
        Id = Id,
        Nombre = Nombre,
        Direccion = Direccion,
        Telefono = Telefono,
        HorarioAtencion = HorarioAtencion,
        Encargado = Encargado,
        Descripcion = Descripcion
    };
}
