using VetSucursales.Views;

namespace VetSucursales;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(SucursalFormPage), typeof(SucursalFormPage));
        Routing.RegisterRoute(nameof(SucursalDetailPage), typeof(SucursalDetailPage));
    }
}
