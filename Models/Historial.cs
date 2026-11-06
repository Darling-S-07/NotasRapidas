using SQLite;

namespace NotasRapidas.Models;

/// <summary>Acciones que se registran en el historial.</summary>
public static class AccionesHistorial
{
    public const string Creada = "Creada";
    public const string Editada = "Editada";
    public const string Eliminada = "Eliminada";
}

public class Historial
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(20)]
    public string Accion { get; set; } = string.Empty;

    // Id de la nota afectada. No es llave foránea: la nota puede haberse eliminado.
    public int NotaId { get; set; }

    // Copia del título al momento de la acción (la nota original puede ya no existir).
    [NotNull, MaxLength(100)]
    public string TituloNota { get; set; } = string.Empty;

    [NotNull]
    public DateTime Fecha { get; set; }

    // Propiedades solo para mostrar en pantalla; [Ignore] evita que SQLite las trate como columnas.
    [Ignore]
    public string Icono => Accion switch
    {
        AccionesHistorial.Creada => "➕",
        AccionesHistorial.Editada => "✏️",
        AccionesHistorial.Eliminada => "🗑️",
        _ => "•"
    };

    [Ignore]
    public string Descripcion => Accion switch
    {
        AccionesHistorial.Creada => "Nota creada",
        AccionesHistorial.Editada => "Nota editada",
        AccionesHistorial.Eliminada => "Nota eliminada",
        _ => Accion
    };
}
