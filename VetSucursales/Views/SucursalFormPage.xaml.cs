using VetSucursales.ViewModels;

namespace VetSucursales.Views;

public partial class SucursalFormPage : ContentPage
{
    public SucursalFormPage(SucursalFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
