using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VetSucursales.Models;
using VetSucursales.Services;
using VetSucursales.Views;

namespace VetSucursales.ViewModels;

[QueryProperty(nameof(Id), "Id")]
public partial class SucursalDetailViewModel : BaseViewModel
{
    private readonly IFirestoreService _firestoreService;

    [ObservableProperty]
    private string? id;

    [ObservableProperty]
    private Sucursal? sucursal;

    public bool HasSucursal => Sucursal is not null;

    public SucursalDetailViewModel(IFirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
    }

    partial void OnSucursalChanged(Sucursal? value) => OnPropertyChanged(nameof(HasSucursal));

    partial void OnIdChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            _ = LoadAsync(value);
    }

    private async Task LoadAsync(string id)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            Sucursal = await _firestoreService.GetSucursalAsync(id);

            if (Sucursal is null)
                ErrorMessage = "No se encontró la sucursal solicitada.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (Sucursal is null)
            return;

        await Shell.Current.GoToAsync($"{nameof(SucursalFormPage)}?Id={Sucursal.Id}");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Sucursal is null || IsBusy)
            return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Eliminar sucursal",
            $"¿Está seguro de que desea eliminar la sucursal \"{Sucursal.Nombre}\"? Esta acción no se puede deshacer.",
            "Eliminar",
            "Cancelar");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            await _firestoreService.DeleteSucursalAsync(Sucursal.Id!);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task BackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
