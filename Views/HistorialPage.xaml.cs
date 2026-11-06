using NotasRapidas.ViewModels;

namespace NotasRapidas.Views;

/// <summary>Code-behind sin lógica: recarga el historial cada vez que la pestaña aparece.</summary>
public partial class HistorialPage : ContentPage
{
    private readonly HistorialViewModel _viewModel;

    public HistorialPage(HistorialViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarHistorialCommand.ExecuteAsync(null);
    }
}
