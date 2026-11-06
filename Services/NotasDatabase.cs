using System.Globalization;
using System.Text;
using NotasRapidas.Models;
using SQLite;

namespace NotasRapidas.Services;

/// Maneja la conexión a SQLite y las operaciones sobre las tablas Nota e Historial.
/// Se registra como Singleton en MauiProgram para compartir una sola conexión.
public class NotasDatabase
{
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    private readonly string _dbPath;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _database;

    public NotasDatabase(string dbPath)
    {
        _dbPath = dbPath;
    }

    // Abre la conexión y crea las tablas la primera vez que se necesita (sin bloquear la UI).
    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_database is not null)
            return _database;

        await _initLock.WaitAsync();
        try
        {
            if (_database is null)
            {
                var connection = new SQLiteAsyncConnection(_dbPath, Flags);
                await connection.CreateTableAsync<Nota>();
                await connection.CreateTableAsync<Historial>();
                _database = connection;
            }

            return _database;
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ===================================================================
    // Notas
    // ===================================================================

    // Read: todas las notas (la más recientemente modificada primero).
    // Si se indica un filtro, se buscan coincidencias en el título, sin distinguir
    // mayúsculas/minúsculas ni tildes ("reunion" encuentra "Reunión").
    public async Task<List<Nota>> ObtenerNotasAsync(string? filtro = null)
    {
        var db = await GetConnectionAsync();
        var notas = await db.Table<Nota>()
                            .OrderByDescending(n => n.FechaModificacion)
                            .ToListAsync();

        if (string.IsNullOrWhiteSpace(filtro))
            return notas;

        string buscado = Normalizar(filtro.Trim());
        return notas.Where(n => Normalizar(n.Titulo).Contains(buscado)).ToList();
    }

    // Read by Id
    public async Task<Nota?> ObtenerNotaPorIdAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.FindAsync<Nota>(id);
    }

    // Create / Update: si Id == 0 inserta, si no actualiza.
    // La nota y su registro en el historial se guardan en una sola transacción:
    // o se guardan los dos o no se guarda ninguno.
    public async Task<Nota> GuardarNotaAsync(Nota nota)
    {
        var db = await GetConnectionAsync();

        bool esNueva = nota.Id == 0;
        var ahora = DateTime.Now;

        if (esNueva)
            nota.FechaCreacion = ahora;
        nota.FechaModificacion = ahora;

        await db.RunInTransactionAsync(conn =>
        {
            if (esNueva)
                conn.Insert(nota);   // tras insertar, nota.Id contiene el ID generado
            else
                conn.Update(nota);

            conn.Insert(new Historial
            {
                Accion = esNueva ? AccionesHistorial.Creada : AccionesHistorial.Editada,
                NotaId = nota.Id,
                TituloNota = nota.Titulo,
                Fecha = ahora
            });
        });

        return nota;
    }

    // Delete (también registra la acción en el historial, en la misma transacción).
    public async Task EliminarNotaAsync(Nota nota)
    {
        var db = await GetConnectionAsync();

        await db.RunInTransactionAsync(conn =>
        {
            conn.Delete(nota);

            conn.Insert(new Historial
            {
                Accion = AccionesHistorial.Eliminada,
                NotaId = nota.Id,
                TituloNota = nota.Titulo,
                Fecha = DateTime.Now
            });
        });
    }

    // ===================================================================
    // Historial
    // ===================================================================

    // Read: del registro más reciente al más antiguo.
    public async Task<List<Historial>> ObtenerHistorialAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Historial>()
                       .OrderByDescending(h => h.Id)
                       .ToListAsync();
    }

    public async Task<int> LimpiarHistorialAsync()
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAllAsync<Historial>();
    }

    // ===================================================================
    // Utilidades
    // ===================================================================

    // Minúsculas y sin tildes, para comparar textos en la búsqueda.
    private static string Normalizar(string texto)
    {
        string descompuesto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);

        foreach (char c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
