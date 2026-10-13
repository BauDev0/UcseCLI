namespace UcseCLI.Core;

/// <summary>
/// Escribe los archivos generados y registra qué se creó, para poder mostrar
/// al final el árbol de la estructura resultante.
/// </summary>
public sealed class EscritorDeArchivos
{
    private readonly string _raiz;
    private readonly List<string> _creados = new();
    private readonly List<string> _omitidos = new();

    public EscritorDeArchivos(string raiz)
    {
        _raiz = raiz;
    }

    /// <summary>Rutas relativas (con '/') de los archivos creados.</summary>
    public IReadOnlyList<string> Creados => _creados;

    /// <summary>Rutas relativas de los archivos que ya existían y no se tocaron.</summary>
    public IReadOnlyList<string> Omitidos => _omitidos;

    /// <summary>
    /// Escribe un archivo de texto creando las carpetas necesarias.
    /// Con <paramref name="sobrescribir"/> en false, un archivo existente no se modifica.
    /// </summary>
    public bool Escribir(string rutaAbsoluta, string contenido, bool sobrescribir = true)
    {
        if (!sobrescribir && File.Exists(rutaAbsoluta))
        {
            _omitidos.Add(Relativa(rutaAbsoluta));
            return false;
        }

        var carpeta = Path.GetDirectoryName(rutaAbsoluta);

        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        // Texto plano con saltos de línea del sistema y sin BOM.
        File.WriteAllText(rutaAbsoluta, Normalizar(contenido));
        _creados.Add(Relativa(rutaAbsoluta));

        return true;
    }

    /// <summary>Crea una carpeta (si no existe) sin registrarla como archivo.</summary>
    public void CrearCarpeta(string rutaAbsoluta) => Directory.CreateDirectory(rutaAbsoluta);

    /// <summary>Borra un archivo generado por las plantillas de dotnet, si está presente.</summary>
    public bool BorrarSiExiste(string rutaAbsoluta)
    {
        if (!File.Exists(rutaAbsoluta))
        {
            return false;
        }

        File.Delete(rutaAbsoluta);

        return true;
    }

    private string Relativa(string rutaAbsoluta)
    {
        var relativa = Path.GetRelativePath(_raiz, rutaAbsoluta);

        return relativa.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string Normalizar(string contenido)
    {
        var texto = contenido.Replace("\r\n", "\n");

        if (!texto.EndsWith('\n'))
        {
            texto += "\n";
        }

        return Environment.NewLine == "\n"
            ? texto
            : texto.Replace("\n", Environment.NewLine);
    }
}
