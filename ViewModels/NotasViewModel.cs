using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotasRapidas.Models;
using NotasRapidas.Services;

namespace NotasRapidas.ViewModels;

/// <summary>
/// ViewModel de la pestaña "Notas": lista, búsqueda por título y acciones
/// de crear / editar / eliminar.
/// </summary>
public partial class NotasViewModel : ObservableObject
{
    private readonly NotasDatabase _database;

    // Se incrementa en cada consulta; permite descartar resultados de búsquedas
    // anteriores que terminen después de una más reciente (al escribir rápido).
    private int _consultaActual;

    public NotasViewModel(NotasDatabase database)
    {
        _database = database;
        notas = new ObservableCollection<Nota>();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarVacio))]
    private ObservableCollection<Nota> notas;

    [ObservableProperty]
    private string tituloLista = "Mis notas";

    // Texto del SearchBar (se enlaza con TwoWay y busca mientras se escribe).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MensajeVacio))]
    private string textoBusqueda = string.Empty;

    public bool MostrarVacio => Notas.Count == 0;

    public string MensajeVacio =>
        string.IsNullOrWhiteSpace(TextoBusqueda)
            ? "Aún no hay notas. Toca «Nueva nota» para crear la primera."
            : "No se encontraron notas con ese título.";

    partial void OnTextoBusquedaChanged(string value)
    {
        _ = CargarNotasAsync();
    }

    // ===================================================================
    // Cargar / buscar (se invoca desde NotasPage.OnAppearing y al escribir en el SearchBar)
    // ===================================================================

    [RelayCommand]
    private async Task CargarNotasAsync()
    {
        int consulta = ++_consultaActual;

        try
        {
            var lista = await _database.ObtenerNotasAsync(TextoBusqueda);

            if (consulta != _consultaActual)
                return; // llegó una búsqueda más reciente

            Notas = new ObservableCollection<Nota>(lista);

            TituloLista = string.IsNullOrWhiteSpace(TextoBusqueda)
                ? $"Mis notas ({Notas.Count})"
                : $"Resultados ({Notas.Count})";
        }
        catch (Exception ex)
        {
            await Dialogos.MostrarAsync("Error", $"No se pudieron cargar las notas: {ex.Message}");
        }
    }

    // ===================================================================
    // Crear / editar (navegan a la ruta de detalle)
    // ===================================================================

    [RelayCommand]
    private Task NuevaNotaAsync() => Shell.Current.GoToAsync(Rutas.NotaDetalle);

    [RelayCommand]
    private Task EditarNotaAsync(Nota? nota)
    {
        if (nota is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync($"{Rutas.NotaDetalle}?Id={nota.Id}");
    }

    // ===================================================================
    // Eliminar (con confirmación)
    // ===================================================================

    [RelayCommand]
    private async Task EliminarNotaAsync(Nota? nota)
    {
        if (nota is null)
            return;

        bool confirmar = await Dialogos.ConfirmarAsync(
            "Eliminar nota",
            $"¿Seguro que deseas eliminar la nota «{nota.Titulo}»? Esta acción no se puede deshacer.",
            "Eliminar",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            await _database.EliminarNotaAsync(nota);
            await CargarNotasAsync();
        }
        catch (Exception ex)
        {
            await Dialogos.MostrarAsync("Error", $"No se pudo eliminar la nota: {ex.Message}");
        }
    }
}
