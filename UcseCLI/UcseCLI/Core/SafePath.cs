namespace UcseCLI.Core;

/// <summary>
/// Resolución de rutas acotada al directorio de trabajo: la herramienta nunca
/// escribe fuera de la carpeta desde la que se la invoca.
/// </summary>
public static class SafePath
{
    /// <summary>
    /// Resuelve <paramref name="rutaRelativa"/> contra el directorio actual.
    /// Devuelve null si el resultado cae fuera del directorio actual
    /// (por ejemplo "../../otra-carpeta" o una ruta absoluta).
    /// </summary>
    public static string? ResolverDentroDelActual(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return null;
        }

        var baseDir = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Directory.GetCurrentDirectory()));

        string resuelto;

        try
        {
            resuelto = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(Path.Combine(baseDir, rutaRelativa)));
        }
        catch (Exception)
        {
            // Caracteres inválidos para el sistema de archivos.
            return null;
        }

        if (string.Equals(resuelto, baseDir, PathComparison))
        {
            return resuelto;
        }

        var prefijo = baseDir + Path.DirectorySeparatorChar;

        return resuelto.StartsWith(prefijo, PathComparison) ? resuelto : null;
    }

    /// <summary>
    /// Comparación de rutas acorde al sistema operativo: Windows y macOS no
    /// distinguen mayúsculas, Linux sí.
    /// </summary>
    private static StringComparison PathComparison =>
        OperatingSystem.IsLinux() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    /// <summary>Ruta relativa legible respecto del directorio actual, para mostrar en consola.</summary>
    public static string ParaMostrar(string rutaCompleta)
    {
        try
        {
            var relativa = Path.GetRelativePath(Directory.GetCurrentDirectory(), rutaCompleta);
            return relativa.StartsWith("..", StringComparison.Ordinal) ? rutaCompleta : relativa;
        }
        catch (Exception)
        {
            return rutaCompleta;
        }
    }
}
