namespace VetSucursales.Models;

public class Encargado
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    public Encargado Clone() => new()
    {
        Nombre = Nombre,
        Apellido = Apellido,
        Telefono = Telefono,
        Correo = Correo
    };
}
