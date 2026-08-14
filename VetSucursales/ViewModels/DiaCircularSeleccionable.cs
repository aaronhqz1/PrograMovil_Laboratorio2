using CommunityToolkit.Mvvm.ComponentModel;
using VetSucursales.Helpers;
using VetSucursales.Models;

namespace VetSucursales.ViewModels;

/// <summary>Un círculo de día en el modal de horario (L, M, M, J, V, S, D), con su estado de selección.</summary>
public partial class DiaCircularSeleccionable : ObservableObject
{
    public DiaSemana Dia { get; }

    public string Inicial => DisplayFormatter.InicialDia(Dia);

    [ObservableProperty]
    private bool isSelected;

    public DiaCircularSeleccionable(DiaSemana dia, bool isSelected = false)
    {
        Dia = dia;
        this.isSelected = isSelected;
    }
}
