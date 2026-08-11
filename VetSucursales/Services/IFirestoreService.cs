using VetSucursales.Models;

namespace VetSucursales.Services;

public interface IFirestoreService
{
    Task<List<Sucursal>> GetSucursalesAsync();

    Task<Sucursal?> GetSucursalAsync(string id);

    Task<Sucursal> AddSucursalAsync(Sucursal sucursal);

    Task UpdateSucursalAsync(Sucursal sucursal);

    Task DeleteSucursalAsync(string id);
}
