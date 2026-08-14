namespace VetSucursales.Helpers;

/// <summary>Las 7 provincias de Costa Rica, usadas en el Picker de Dirección y para generar datos de prueba.</summary>
public static class CostaRicaProvincias
{
    public static readonly IReadOnlyList<string> Todas = new[]
    {
        "San José", "Alajuela", "Cartago", "Heredia", "Guanacaste", "Puntarenas", "Limón"
    };
}
