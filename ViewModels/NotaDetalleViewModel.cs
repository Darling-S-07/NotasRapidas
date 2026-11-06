using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotasRapidas.Models;
using NotasRapidas.Services;

namespace NotasRapidas.ViewModels;

/// <summary>
/// ViewModel de la ruta de detalle: sirve para crear una nota (Id = 0)
/// y para editar o eliminar una existente (Id recibido por Shell).
/// </summary>
[QueryProperty(nameof(NotaId), "Id")]
public partial class NotaDetalleViewModel : ObservableObject
{
    public const int MaxTitulo = 100;

    private readonly NotasDatabase _database;

    private Nota? _notaOriginal;
    private bool _cargando;
    private bool _tituloTocado;

    public NotaDetalleViewModel(NotasDatabase database)
    {
        _database = database;
    }

    // Llega por la ruta: NotaDetalle?Id=5. Sin parámetro queda en 0 (nota nueva).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    [NotifyPropertyChangedFor(nameof(TituloPagina))]
    private int notaId;

    public bool EsEdicion => NotaId > 0;

    public string TituloPagina => EsEdicion ? "Editar nota" : "Nueva nota";

    // ===================================================================
    // Campos del formulario
    // ===================================================================

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    [NotifyPropertyChangedFor(nameof(ErrorTitulo))]
    [NotifyPropertyChangedFor(nameof(MostrarErrorTitulo))]
    [NotifyPropertyChangedFor(nameof(ContadorTitulo))]
    private string titulo = string.Empty;

    [ObservableProperty]
    private string contenido = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    private bool isBusy;

    // ===================================================================
    // Validación del título: obligatorio y máximo 100 caracteres
    // ===================================================================

    public string ErrorTitulo
    {
        get
        {
            string limpio = (Titulo ?? string.Empty).Trim();

            if (limpio.Length == 0)
                return "El título es obligatorio.";

            if (limpio.Length > MaxTitulo)
                return $"El título no puede superar {MaxTitulo} caracteres.";

            return string.Empty;
        }
    }

    // El error se muestra solo después de que el usuario escribe algo,
    // para no mostrarlo en rojo al abrir una nota nueva.
    public bool MostrarErrorTitulo => _tituloTocado && ErrorTitulo.Length > 0;

    public string ContadorTitulo => $"{(Titulo ?? string.Empty).Length}/{MaxTitulo}";

    partial void OnTituloChanged(string value)
    {
        if (!_cargando)
            _tituloTocado = true;
    }

    // ===================================================================
    // Cargar la nota a editar (cuando Shell entrega el Id)
    // ===================================================================

    partial void OnNotaIdChanged(int value)
    {
        if (value > 0)
            _ = CargarNotaAsync(value);
    }

    private async Task CargarNotaAsync(int id)
    {
        try
        {
            var nota = await _database.ObtenerNotaPorIdAsync(id);

            if (nota is null)
            {
                await Dialogos.MostrarAsync("Nota no encontrada", "La nota ya no existe.");
                await Shell.Current.GoToAsync("..");
                return;
            }

            _notaOriginal = nota;

            _cargando = true;
            Titulo = nota.Titulo;
            Contenido = nota.Contenido;
            _tituloTocado = false;
            _cargando = false;

            OnPropertyChanged(nameof(MostrarErrorTitulo));
        }
        catch (Exception ex)
        {
            _cargando = false;
            await Dialogos.MostrarAsync("Error", $"No se pudo cargar la nota: {ex.Message}");
        }
    }

    // ===================================================================
    // Guardar (CanExecute: el título debe ser válido)
    // ===================================================================

    private bool PuedeGuardar() => !IsBusy && ErrorTitulo.Length == 0;

    [RelayCommand(CanExecute = nameof(PuedeGuardar))]
    private async Task GuardarAsync()
    {
        if (!PuedeGuardar())
            return;

        IsBusy = true;
        try
        {
            var nota = _notaOriginal ?? new Nota();
            nota.Titulo = Titulo.Trim();
            nota.Contenido = (Contenido ?? string.Empty).Trim();

            // Inserta o actualiza y registra la acción en el historial.
            await _database.GuardarNotaAsync(nota);

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Dialogos.MostrarAsync("Error", $"Ocurrió un error al guardar la nota: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ===================================================================
    // Eliminar (solo en modo edición, con confirmación)
    // ===================================================================

    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (_notaOriginal is null)
            return;

        bool confirmar = await Dialogos.ConfirmarAsync(
            "Eliminar nota",
            $"¿Seguro que deseas eliminar la nota «{_notaOriginal.Titulo}»? Esta acción no se puede deshacer.",
            "Eliminar",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            await _database.EliminarNotaAsync(_notaOriginal);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Dialogos.MostrarAsync("Error", $"No se pudo eliminar la nota: {ex.Message}");
        }
    }

    [RelayCommand]
    private Task CancelarAsync() => Shell.Current.GoToAsync("..");
}
