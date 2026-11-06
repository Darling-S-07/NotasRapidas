namespace NotasRapidas.Services;

/// <summary>
/// Helper de diálogos compartido por los ViewModels: no conocen la Page,
/// usan la ventana activa.
/// </summary>
public static class Dialogos
{
    private static Page? PaginaActual => Application.Current?.Windows.FirstOrDefault()?.Page;

    public static async Task MostrarAsync(string titulo, string mensaje, string aceptar = "OK")
    {
        var page = PaginaActual;
        if (page is null)
            return;

        await page.DisplayAlertAsync(titulo, mensaje, aceptar);
    }

    /// <summary>Devuelve true si el usuario pulsa el botón de aceptar.</summary>
    public static Task<bool> ConfirmarAsync(string titulo, string mensaje, string aceptar, string cancelar)
    {
        var page = PaginaActual;
        if (page is null)
            return Task.FromResult(false);

        return page.DisplayAlertAsync(titulo, mensaje, aceptar, cancelar);
    }
}
