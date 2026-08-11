using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VetSucursales.Services;

namespace VetSucursales.ViewModels;

[QueryProperty(nameof(Id), "Id")]
public partial class SucursalFormViewModel : BaseViewModel
{
    private readonly IFirestoreService _firestoreService;

    [ObservableProperty]
    private string? id;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string direccion = string.Empty;

    [ObservableProperty]
    private string telefono = string.Empty;

    [ObservableProperty]
    private string horarioAtencion = string.Empty;

    [ObservableProperty]
    private string encargado = string.Empty;

    [ObservableProperty]
    private string descripcion = string.Empty;

    [ObservableProperty]
    private string nombreError = string.Empty;

    [ObservableProperty]
    private string direccionError = string.Empty;

    [ObservableProperty]
    private string telefonoError = string.Empty;

    [ObservableProperty]
    private string horarioError = string.Empty;

    [ObservableProperty]
    private string encargadoError = string.Empty;

    [ObservableProperty]
    private string descripcionError = string.Empty;

    public bool IsEdit => !string.IsNullOrWhiteSpace(Id);

    public string Title => IsEdit ? "Editar sucursal" : "Registrar sucursal";

    public SucursalFormViewModel(IFirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
    }

    partial void OnIdChanged(string? value)
    {
        OnPropertyChanged(nameof(IsEdit));
        OnPropertyChanged(nameof(Title));

        if (!string.IsNullOrWhiteSpace(value))
            _ = LoadAsync(value);
    }

    private async Task LoadAsync(string id)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var sucursal = await _firestoreService.GetSucursalAsync(id);
            if (sucursal is null)
            {
                ErrorMessage = "No se encontró la sucursal solicitada.";
                return;
            }

            Nombre = sucursal.Nombre;
            Direccion = sucursal.Direccion;
            Telefono = sucursal.Telefono;
            HorarioAtencion = sucursal.HorarioAtencion;
            Encargado = sucursal.Encargado;
            Descripcion = sucursal.Descripcion;
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

    private bool Validate()
    {
        NombreError = string.IsNullOrWhiteSpace(Nombre) ? "El nombre es obligatorio." : string.Empty;
        DireccionError = string.IsNullOrWhiteSpace(Direccion) ? "La dirección es obligatoria." : string.Empty;
        EncargadoError = string.IsNullOrWhiteSpace(Encargado) ? "El encargado es obligatorio." : string.Empty;
        HorarioError = string.IsNullOrWhiteSpace(HorarioAtencion) ? "El horario de atención es obligatorio." : string.Empty;
        DescripcionError = string.IsNullOrWhiteSpace(Descripcion) ? "La descripción es obligatoria." : string.Empty;

        if (string.IsNullOrWhiteSpace(Telefono))
        {
            TelefonoError = "El teléfono es obligatorio.";
        }
        else if (Telefono.Count(char.IsDigit) < 8)
        {
            TelefonoError = "Ingrese un teléfono válido (mínimo 8 dígitos).";
        }
        else
        {
            TelefonoError = string.Empty;
        }

        return string.IsNullOrEmpty(NombreError)
            && string.IsNullOrEmpty(DireccionError)
            && string.IsNullOrEmpty(TelefonoError)
            && string.IsNullOrEmpty(HorarioError)
            && string.IsNullOrEmpty(EncargadoError)
            && string.IsNullOrEmpty(DescripcionError);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (!Validate())
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var sucursal = new Models.Sucursal
            {
                Id = Id,
                Nombre = Nombre,
                Direccion = Direccion,
                Telefono = Telefono,
                HorarioAtencion = HorarioAtencion,
                Encargado = Encargado,
                Descripcion = Descripcion,
            };

            if (IsEdit)
                await _firestoreService.UpdateSucursalAsync(sucursal);
            else
                await _firestoreService.AddSucursalAsync(sucursal);

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
    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
