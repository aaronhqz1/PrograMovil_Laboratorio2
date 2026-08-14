using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Shapes;
using VetSucursales.Helpers;
using VetSucursales.Models;
using VetSucursales.Services;
using VetSucursales.Views;

namespace VetSucursales.ViewModels;

[QueryProperty(nameof(Id), "Id")]
public partial class SucursalFormViewModel : BaseViewModel
{
    // Acepta dígitos, espacios, guiones, paréntesis y un "+" inicial opcional (formato flexible
    // para números de Costa Rica u otros países), con al menos 8 dígitos en total.
    private static readonly Regex TelefonoRegex = new(@"^\+?[0-9\s\-\(\)]{7,20}$", RegexOptions.Compiled);

    // Código postal: solo dígitos, máximo 5 (el Entry además limita la escritura con MaxLength).
    private static readonly Regex CodigoPostalRegex = new(@"^[0-9]{1,5}$", RegexOptions.Compiled);

    private static readonly Regex CorreoRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly IFirestoreService _firestoreService;

    /// <summary>True una vez que el horario se guardó al menos una vez desde el modal (o se cargó
    /// de una sucursal existente). Se usa para exigir que el horario haya sido configurado.</summary>
    private bool _horarioConfigurado;

    [ObservableProperty]
    private string? id;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string direccion1 = string.Empty;

    [ObservableProperty]
    private string direccion2 = string.Empty;

    [ObservableProperty]
    private string ciudad = string.Empty;

    [ObservableProperty]
    private string estadoProvincia = string.Empty;

    [ObservableProperty]
    private string codigoPostal = string.Empty;

    [ObservableProperty]
    private string telefono = string.Empty;

    [ObservableProperty]
    private ObservableCollection<HorarioDia> horario = new(Models.Sucursal.CrearHorarioSemanaVacio());

    [ObservableProperty]
    private ObservableCollection<HorarioResumenItem> horarioResumen = new();

    [ObservableProperty]
    private string encargadoNombre = string.Empty;

    [ObservableProperty]
    private string encargadoApellido = string.Empty;

    [ObservableProperty]
    private string encargadoTelefono = string.Empty;

    [ObservableProperty]
    private string encargadoCorreo = string.Empty;

    [ObservableProperty]
    private string descripcion = string.Empty;

    [ObservableProperty]
    private string nombreError = string.Empty;

    [ObservableProperty]
    private string direccion1Error = string.Empty;

    [ObservableProperty]
    private string ciudadError = string.Empty;

    [ObservableProperty]
    private string estadoProvinciaError = string.Empty;

    [ObservableProperty]
    private string codigoPostalError = string.Empty;

    [ObservableProperty]
    private string telefonoError = string.Empty;

    [ObservableProperty]
    private string horarioError = string.Empty;

    [ObservableProperty]
    private string encargadoNombreError = string.Empty;

    [ObservableProperty]
    private string encargadoApellidoError = string.Empty;

    [ObservableProperty]
    private string encargadoTelefonoError = string.Empty;

    [ObservableProperty]
    private string encargadoCorreoError = string.Empty;

    [ObservableProperty]
    private string descripcionError = string.Empty;

    public bool IsEdit => !string.IsNullOrWhiteSpace(Id);

    public string Title => IsEdit ? "Editar sucursal" : "Registrar sucursal";

    /// <summary>Las 7 provincias de Costa Rica, para el selector de Provincia del formulario.</summary>
    public List<string> Provincias { get; } = new()
    {
        "San José", "Alajuela", "Cartago", "Heredia", "Guanacaste", "Puntarenas", "Limón"
    };

    public SucursalFormViewModel(IFirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
        RebuildHorarioResumen();
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
            Direccion1 = sucursal.Direccion.Direccion1;
            Direccion2 = sucursal.Direccion.Direccion2;
            Ciudad = sucursal.Direccion.Ciudad;
            EstadoProvincia = sucursal.Direccion.Estado;
            CodigoPostal = sucursal.Direccion.CodigoPostal;
            Telefono = sucursal.Telefono;
            Horario = new ObservableCollection<HorarioDia>(sucursal.Horario.Select(h => h.Clone()));
            EncargadoNombre = sucursal.Encargado.Nombre;
            EncargadoApellido = sucursal.Encargado.Apellido;
            EncargadoTelefono = sucursal.Encargado.Telefono;
            EncargadoCorreo = sucursal.Encargado.Correo;
            Descripcion = sucursal.Descripcion;

            _horarioConfigurado = true;
            RebuildHorarioResumen();
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

    private void RebuildHorarioResumen() =>
        HorarioResumen = new ObservableCollection<HorarioResumenItem>(HorarioResumenItem.DesdeHorario(Horario));

    [RelayCommand]
    private Task EditarHorarioAsync() => AbrirModalHorarioAsync(null);

    [RelayCommand]
    private Task EditarDiaAsync(HorarioDia? dia)
    {
        if (dia is null)
            return Task.CompletedTask;

        var diasConMismoHorario = Horario.Where(h => h.TieneMismoHorarioQue(dia)).Select(h => h.DiaSemana).ToList();
        return AbrirModalHorarioAsync(diasConMismoHorario);
    }

    private async Task AbrirModalHorarioAsync(List<DiaSemana>? preseleccion)
    {
        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var popup = new HorarioEditorPopup(Horario.ToList(), preseleccion);
        var options = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            Shape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            PageOverlayColor = Colors.Black.WithAlpha(0.45f),
        };

        var resultado = await page.ShowPopupAsync<HorarioSeleccion>(popup, options, CancellationToken.None);
        if (resultado.WasDismissedByTappingOutsideOfPopup || resultado.Result is null)
            return;

        AplicarSeleccionHorario(resultado.Result);
    }

    private void AplicarSeleccionHorario(HorarioSeleccion seleccion)
    {
        foreach (var diaSemana in seleccion.Dias)
        {
            var dia = Horario.First(h => h.DiaSemana == diaSemana);
            dia.Estado = seleccion.Estado;
            dia.Rangos = seleccion.Estado == EstadoHorarioDia.HorarioPersonalizado
                ? seleccion.Rangos.Select(r => r.Clone()).ToList()
                : new List<RangoHorario>();
        }

        _horarioConfigurado = true;
        HorarioError = string.Empty;
        RebuildHorarioResumen();
    }

    /// <summary>Comando enlazado al evento Unfocused de cada campo (ver XAML) para validar en tiempo real.</summary>
    [RelayCommand]
    private void ValidateFields() => Validate();

    /// <summary>Valida todos los campos y actualiza los mensajes de error inline. Se ejecuta tanto
    /// al perder el foco de un campo (<see cref="ValidateFieldsCommand"/>) como al intentar guardar.</summary>
    private bool Validate()
    {
        NombreError = string.IsNullOrWhiteSpace(Nombre) ? "El nombre es obligatorio." : string.Empty;
        Direccion1Error = string.IsNullOrWhiteSpace(Direccion1) ? "La dirección es obligatoria." : string.Empty;
        CiudadError = string.IsNullOrWhiteSpace(Ciudad) ? "La ciudad es obligatoria." : string.Empty;
        EstadoProvinciaError = string.IsNullOrWhiteSpace(EstadoProvincia) ? "El estado/provincia es obligatorio." : string.Empty;

        if (string.IsNullOrWhiteSpace(CodigoPostal))
            CodigoPostalError = "El código postal es obligatorio.";
        else if (!CodigoPostalRegex.IsMatch(CodigoPostal))
            CodigoPostalError = "Ingrese solo números (máximo 5 dígitos).";
        else
            CodigoPostalError = string.Empty;

        TelefonoError = ValidarTelefono(Telefono);

        HorarioError = _horarioConfigurado ? string.Empty : "Debe configurar el horario de atención.";

        EncargadoNombreError = string.IsNullOrWhiteSpace(EncargadoNombre) ? "El nombre del encargado es obligatorio." : string.Empty;
        EncargadoApellidoError = string.IsNullOrWhiteSpace(EncargadoApellido) ? "El apellido del encargado es obligatorio." : string.Empty;
        EncargadoTelefonoError = ValidarTelefono(EncargadoTelefono);

        if (string.IsNullOrWhiteSpace(EncargadoCorreo))
            EncargadoCorreoError = "El correo del encargado es obligatorio.";
        else if (!CorreoRegex.IsMatch(EncargadoCorreo))
            EncargadoCorreoError = "Ingrese un correo electrónico válido.";
        else
            EncargadoCorreoError = string.Empty;

        DescripcionError = string.IsNullOrWhiteSpace(Descripcion) ? "La descripción es obligatoria." : string.Empty;

        return string.IsNullOrEmpty(NombreError)
            && string.IsNullOrEmpty(Direccion1Error)
            && string.IsNullOrEmpty(CiudadError)
            && string.IsNullOrEmpty(EstadoProvinciaError)
            && string.IsNullOrEmpty(CodigoPostalError)
            && string.IsNullOrEmpty(TelefonoError)
            && string.IsNullOrEmpty(HorarioError)
            && string.IsNullOrEmpty(EncargadoNombreError)
            && string.IsNullOrEmpty(EncargadoApellidoError)
            && string.IsNullOrEmpty(EncargadoTelefonoError)
            && string.IsNullOrEmpty(EncargadoCorreoError)
            && string.IsNullOrEmpty(DescripcionError);
    }

    private static string ValidarTelefono(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return "El teléfono es obligatorio.";

        if (!TelefonoRegex.IsMatch(telefono) || telefono.Count(char.IsDigit) < 8)
            return "Ingrese un teléfono válido (mínimo 8 dígitos; se permiten espacios, guiones y paréntesis).";

        return string.Empty;
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
                Direccion = new Direccion
                {
                    Direccion1 = Direccion1,
                    Direccion2 = Direccion2,
                    Ciudad = Ciudad,
                    Estado = EstadoProvincia,
                    CodigoPostal = CodigoPostal,
                },
                Telefono = Telefono,
                Horario = Horario.ToList(),
                Encargado = new Encargado
                {
                    Nombre = EncargadoNombre,
                    Apellido = EncargadoApellido,
                    Telefono = EncargadoTelefono,
                    Correo = EncargadoCorreo,
                },
                Descripcion = Descripcion,
            };

            if (IsEdit)
                await _firestoreService.UpdateSucursalAsync(sucursal);
            else
                await _firestoreService.AddSucursalAsync(sucursal);

            await Toast.Make(IsEdit ? "Sucursal actualizada correctamente." : "Sucursal registrada correctamente.").Show();
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
