using Microsoft.Extensions.Logging;
using NotasRapidas.Services;
using NotasRapidas.ViewModels;
using NotasRapidas.Views;

namespace NotasRapidas
{
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

            // ===================================================================
            // Registro de servicios, ViewModels y Views (arquitectura MVVM)
            // ===================================================================

            // Servicio de datos: Singleton para compartir una única conexión SQLite.
            builder.Services.AddSingleton(_ =>
                new NotasDatabase(Path.Combine(FileSystem.AppDataDirectory, "notas_rapidas.db3")));

            // Shell
            builder.Services.AddSingleton<AppShell>();

            // ViewModels y Pages: Transient, una instancia nueva por cada navegación.
            builder.Services.AddTransient<NotasViewModel>();
            builder.Services.AddTransient<NotasPage>();

            builder.Services.AddTransient<HistorialViewModel>();
            builder.Services.AddTransient<HistorialPage>();

            builder.Services.AddTransient<NotaDetalleViewModel>();
            builder.Services.AddTransient<NotaDetallePage>();

            return builder.Build();
        }
    }
}
