using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VetSucursales.Helpers;
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

    /// <summary>Solo true en builds Debug: controla si se muestra el botón para generar datos de
    /// prueba masivos. En Release queda oculto para no exponerlo en la entrega final.</summary>
#if DEBUG
    public bool CanSeedTestData => true;
#else
    public bool CanSeedTestData => false;
#endif

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

    /// <summary>Genera N sucursales de prueba (dirección, horario y encargado variados) y las
    /// guarda en Firestore, para pruebas masivas. Solo disponible en builds Debug.</summary>
    [RelayCommand]
    private async Task SeedTestDataAsync()
    {
        if (IsBusy)
            return;

        string? input = await Shell.Current.DisplayPromptAsync(
            "Generar datos de prueba",
            "¿Cuántas sucursales de prueba quieres crear?",
            "Crear",
            "Cancelar",
            initialValue: "20",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out int cantidad) || cantidad <= 0)
            return;

        cantidad = Math.Min(cantidad, 200);

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            foreach (var sucursal in TestDataGenerator.Generar(cantidad))
                await _firestoreService.AddSucursalAsync(sucursal);

            await Toast.Make($"{cantidad} sucursales de prueba creadas.").Show();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }

        await LoadAsync();
    }
}
