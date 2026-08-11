using VetSucursales.ViewModels;

namespace VetSucursales.Views;

public partial class SucursalListPage : ContentPage
{
    private readonly SucursalListViewModel _viewModel;

    public SucursalListPage(SucursalListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
