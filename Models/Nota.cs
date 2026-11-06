using SQLite;

namespace NotasRapidas.Models;

public class Nota
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Título obligatorio, máximo 100 caracteres (también se valida en el ViewModel).
    [NotNull, MaxLength(100)]
    public string Titulo { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    [NotNull]
    public DateTime FechaCreacion { get; set; }

    [NotNull]
    public DateTime FechaModificacion { get; set; }
}
