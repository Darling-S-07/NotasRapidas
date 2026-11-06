using NotasRapidas.Services;
using NotasRapidas.Views;

namespace NotasRapidas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Ruta de detalle: no está en las pestañas, se abre con GoToAsync.
            Routing.RegisterRoute(Rutas.NotaDetalle, typeof(NotaDetallePage));
        }
    }
}
