using CommunityToolkit.Maui.Views;
using VetSucursales.Helpers;
using VetSucursales.Models;
using VetSucursales.ViewModels;

namespace VetSucursales.Views;

public partial class HorarioEditorPopup : Popup<HorarioSeleccion>
{
    private readonly HorarioEditorPopupViewModel _viewModel;

    public HorarioEditorPopup(List<HorarioDia> horarioSemana, IEnumerable<DiaSemana>? preseleccion = null)
    {
        InitializeComponent();

        _viewModel = new HorarioEditorPopupViewModel(horarioSemana, preseleccion);
        BindingContext = _viewModel;
    }

    // Se cierra sin resultado (null); SucursalFormViewModel trata esto igual que un tap fuera del popup: no aplica cambios.
    private async void OnCancelarClicked(object? sender, EventArgs e) => await CloseAsync(null!);

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (_viewModel.TryConstruirResultado(out var resultado))
            await CloseAsync(resultado);
    }
}
