using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotasRapidas.Models;
using NotasRapidas.Services;

namespace NotasRapidas.ViewModels;

/// <summary>ViewModel de la pestaña "Historial": muestra cada acción registrada.</summary>
public partial class HistorialViewModel : ObservableObject
{
    private readonly NotasDatabase _database;

    public HistorialViewModel(NotasDatabase database)
    {
        _database = database;
        registros = new ObservableCollection<Historial>();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarVacio))]
    private ObservableCollection<Historial> registros;

    [ObservableProperty]
    private string tituloLista = "Historial de acciones";

    public bool MostrarVacio => Registros.Count == 0;

    // Se invoca desde HistorialPage.OnAppearing, así siempre muestra lo más reciente.
    [RelayCommand]
    private async Task CargarHistorialAsync()
    {
        try
        {
            var lista = await _database.ObtenerHistorialAsync();
            Registros = new ObservableCollection<Historial>(lista);
            TituloLista = $"Historial de acciones ({Registros.Count})";
        }
        catch (Exception ex)
        {
            await Dialogos.MostrarAsync("Error", $"No se pudo cargar el historial: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LimpiarHistorialAsync()
    {
        if (Registros.Count == 0)
            return;

        bool confirmar = await Dialogos.ConfirmarAsync(
            "Limpiar historial",
            "Se borrarán todos los registros del historial (las notas no se tocan). ¿Continuar?",
            "Limpiar",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            await _database.LimpiarHistorialAsync();
            await CargarHistorialAsync();
        }
        catch (Exception ex)
        {
            await Dialogos.MostrarAsync("Error", $"No se pudo limpiar el historial: {ex.Message}");
        }
    }
}
