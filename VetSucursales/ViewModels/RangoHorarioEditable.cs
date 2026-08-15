using CommunityToolkit.Mvvm.ComponentModel;

namespace VetSucursales.ViewModels;

/// <summary>Un bloque Apertura/Cierre editable dentro del modal de horario.</summary>
public partial class RangoHorarioEditable : ObservableObject
{
    [ObservableProperty]
    private TimeSpan apertura = new(9, 0, 0);

    [ObservableProperty]
    private TimeSpan cierre = new(17, 0, 0);

    [ObservableProperty]
    private string error = string.Empty;

    partial void OnAperturaChanged(TimeSpan value) => Error = string.Empty;

    partial void OnCierreChanged(TimeSpan value) => Error = string.Empty;
}
