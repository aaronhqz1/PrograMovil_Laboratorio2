using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VetSucursales.Models;
using VetSucursales.Services;
using VetSucursales.Views;

namespace VetSucursales.ViewModels;

public partial class SucursalListViewModel : BaseViewModel
{
    private readonly IFirestoreService _firestoreService;

    public ObservableCollection<Sucursal> Sucursales { get; } = new();

    [ObservableProperty]
    private bool isEmpty;

    public SucursalListViewModel(IFirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var sucursales = await _firestoreService.GetSucursalesAsync();

            Sucursales.Clear();
            foreach (var sucursal in sucursales)
                Sucursales.Add(sucursal);

            IsEmpty = Sucursales.Count == 0;
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
    private static async Task AddAsync()
    {
        await Shell.Current.GoToAsync(nameof(SucursalFormPage));
    }

    [RelayCommand]
    private static async Task SelectAsync(Sucursal? sucursal)
    {
        if (sucursal is null)
            return;

        await Shell.Current.GoToAsync($"{nameof(SucursalDetailPage)}?Id={sucursal.Id}");
    }
}
