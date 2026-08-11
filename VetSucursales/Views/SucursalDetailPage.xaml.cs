using VetSucursales.ViewModels;

namespace VetSucursales.Views;

public partial class SucursalDetailPage : ContentPage
{
    public SucursalDetailPage(SucursalDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
