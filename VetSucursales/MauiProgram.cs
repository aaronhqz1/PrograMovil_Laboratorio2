using Microsoft.Extensions.Logging;
using VetSucursales.Services;
using VetSucursales.ViewModels;
using VetSucursales.Views;

namespace VetSucursales;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IFirestoreService, FirestoreService>();

        builder.Services.AddTransient<SucursalListViewModel>();
        builder.Services.AddTransient<SucursalFormViewModel>();
        builder.Services.AddTransient<SucursalDetailViewModel>();

        builder.Services.AddTransient<SucursalListPage>();
        builder.Services.AddTransient<SucursalFormPage>();
        builder.Services.AddTransient<SucursalDetailPage>();

        return builder.Build();
    }
}
