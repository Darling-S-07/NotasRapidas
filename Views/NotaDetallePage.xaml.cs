using NotasRapidas.ViewModels;

namespace NotasRapidas.Views;

/// <summary>
/// Code-behind sin lógica de negocio: solo conecta el BindingContext.
/// Shell entrega el parámetro Id directamente al ViewModel ([QueryProperty]).
/// </summary>
public partial class NotaDetallePage : ContentPage
{
    public NotaDetallePage(NotaDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
