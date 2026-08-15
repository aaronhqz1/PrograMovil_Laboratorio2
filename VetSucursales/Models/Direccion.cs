namespace VetSucursales.Models;

public class Direccion
{
    public string Direccion1 { get; set; } = string.Empty;

    public string Direccion2 { get; set; } = string.Empty;

    public string Ciudad { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string CodigoPostal { get; set; } = string.Empty;

    public Direccion Clone() => new()
    {
        Direccion1 = Direccion1,
        Direccion2 = Direccion2,
        Ciudad = Ciudad,
        Estado = Estado,
        CodigoPostal = CodigoPostal
    };
}
