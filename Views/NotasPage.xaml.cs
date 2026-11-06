using NotasRapidas.ViewModels;

namespace NotasRapidas.Views;

/// <summary>
/// Code-behind sin lógica de negocio: conecta el BindingContext (inyectado por DI)
/// y recarga la lista cada vez que la pestaña aparece (por ejemplo, al volver del detalle).
/// </summary>
public partial class NotasPage : ContentPage
{
    private readonly NotasViewModel _viewModel;

    public NotasPage(NotasViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarNotasCommand.ExecuteAsync(null);
    }
}
