using Microsoft.Extensions.DependencyInjection;

namespace NotasRapidas
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // El Shell se resuelve desde el contenedor de inyección de dependencias (MauiProgram.cs).
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            return new Window(shell);
        }
    }
}
