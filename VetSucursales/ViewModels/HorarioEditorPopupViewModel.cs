using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VetSucursales.Helpers;
using VetSucursales.Models;

namespace VetSucursales.ViewModels;

/// <summary>ViewModel del modal "Selecciona los días y el horario". No navega ni cierra el popup
/// (eso lo hace el code-behind, que es quien tiene la instancia de Popup&lt;T&gt;); solo mantiene el
/// estado de la selección y valida/arma el resultado.</summary>
public partial class HorarioEditorPopupViewModel : ObservableObject
{
    public ObservableCollection<DiaCircularSeleccionable> Dias { get; }

    public ObservableCollection<RangoHorarioEditable> Rangos { get; } = new();

    [ObservableProperty]
    private bool es24Horas;

    [ObservableProperty]
    private bool esCerrado;

    [ObservableProperty]
    private string diasError = string.Empty;

    /// <summary>True cuando ninguno de los dos checkboxes está marcado: se muestran los bloques de Apertura/Cierre.</summary>
    public bool MostrarBloqueHorario => !Es24Horas && !EsCerrado;

    public HorarioEditorPopupViewModel(List<HorarioDia> horarioSemana, IEnumerable<DiaSemana>? preseleccion = null)
    {
        var preseleccionados = (preseleccion ?? Enumerable.Empty<DiaSemana>()).ToHashSet();

        Dias = new ObservableCollection<DiaCircularSeleccionable>(
            Enum.GetValues<DiaSemana>().Select(dia => new DiaCircularSeleccionable(dia, preseleccionados.Contains(dia))));

        var plantilla = preseleccionados.Count > 0
            ? horarioSemana.FirstOrDefault(h => preseleccionados.Contains(h.DiaSemana))
            : null;

        if (plantilla is not null)
        {
            Es24Horas = plantilla.Estado == EstadoHorarioDia.Abierto24h;
            EsCerrado = plantilla.Estado == EstadoHorarioDia.Cerrado;

            if (plantilla.Estado == EstadoHorarioDia.HorarioPersonalizado && plantilla.Rangos.Count > 0)
            {
                foreach (var rango in plantilla.Rangos)
                    Rangos.Add(new RangoHorarioEditable { Apertura = rango.HoraInicio, Cierre = rango.HoraCierre });
            }
        }

        if (Rangos.Count == 0)
            Rangos.Add(new RangoHorarioEditable());
    }

    [RelayCommand]
    private void ToggleDia(DiaCircularSeleccionable dia)
    {
        dia.IsSelected = !dia.IsSelected;
        DiasError = string.Empty;
    }

    partial void OnEs24HorasChanged(bool value)
    {
        if (value)
            EsCerrado = false;

        OnPropertyChanged(nameof(MostrarBloqueHorario));
    }

    partial void OnEsCerradoChanged(bool value)
    {
        if (value)
            Es24Horas = false;

        OnPropertyChanged(nameof(MostrarBloqueHorario));
    }

    [RelayCommand]
    private void AgregarHorario()
    {
        var ultimo = Rangos.LastOrDefault();
        var apertura = ultimo?.Cierre ?? new TimeSpan(9, 0, 0);
        Rangos.Add(new RangoHorarioEditable { Apertura = apertura, Cierre = apertura.Add(TimeSpan.FromHours(1)) });
    }

    [RelayCommand]
    private void QuitarRango(RangoHorarioEditable rango)
    {
        if (Rangos.Count > 1)
            Rangos.Remove(rango);
    }

    /// <summary>Valida la selección actual y arma el resultado a devolver. Devuelve false si hay
    /// errores de validación (ya reflejados en DiasError / RangoHorarioEditable.Error).</summary>
    public bool TryConstruirResultado(out HorarioSeleccion? resultado)
    {
        resultado = null;

        var diasSeleccionados = Dias.Where(d => d.IsSelected).Select(d => d.Dia).ToList();
        if (diasSeleccionados.Count == 0)
        {
            DiasError = "Selecciona al menos un día.";
            return false;
        }

        DiasError = string.Empty;

        if (!MostrarBloqueHorario)
        {
            resultado = new HorarioSeleccion
            {
                Dias = diasSeleccionados,
                Estado = Es24Horas ? EstadoHorarioDia.Abierto24h : EstadoHorarioDia.Cerrado,
                Rangos = new List<RangoHorario>()
            };
            return true;
        }

        bool esValido = true;
        foreach (var rango in Rangos)
        {
            if (rango.Cierre <= rango.Apertura)
            {
                rango.Error = "El cierre debe ser posterior a la apertura.";
                esValido = false;
            }
            else
            {
                rango.Error = string.Empty;
            }
        }

        if (!esValido)
            return false;

        resultado = new HorarioSeleccion
        {
            Dias = diasSeleccionados,
            Estado = EstadoHorarioDia.HorarioPersonalizado,
            Rangos = Rangos.Select(r => new RangoHorario { HoraInicio = r.Apertura, HoraCierre = r.Cierre }).ToList()
        };
        return true;
    }
}
