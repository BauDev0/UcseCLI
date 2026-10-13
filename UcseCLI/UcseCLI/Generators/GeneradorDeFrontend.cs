using UcseCLI.Core;
using UcseCLI.Templates;

namespace UcseCLI.Generators;

/// <summary>
/// Genera el frontend de ejemplo de la Unidad 7: HTML, CSS y JavaScript sin
/// frameworks, consumiendo la Web API con fetch.
/// </summary>
public static class GeneradorDeFrontend
{
    public static int Crear(string carpetaRelativa, string? urlApi, string entidadPedida)
    {
        var destino = SafePath.ResolverDentroDelActual(carpetaRelativa);

        if (destino is null)
        {
            Ui.Error("La carpeta del frontend tiene que estar dentro del directorio actual.");
            return 1;
        }

        var entidad = Naming.APascalCase(entidadPedida);

        if (!Naming.EsIdentificadorValido(entidad))
        {
            Ui.Error($"Nombre de entidad inválido: '{entidadPedida}'.");
            return 1;
        }

        var proyectoApi = BuscarProyectoApi() ?? "MiSolucion.Api";
        var url = string.IsNullOrWhiteSpace(urlApi)
            ? $"http://localhost:5000/api/{Naming.ACamelCase(entidad)}"
            : urlApi.TrimEnd('/');

        var nombre = Path.GetFileName(Path.TrimEndingDirectorySeparator(destino));

        Directory.CreateDirectory(destino);

        var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());

        (string Archivo, string Plantilla)[] archivos =
        {
            ("index.html", PlantillasFrontend.IndexHtml),
            ("styles.css", PlantillasFrontend.StylesCss),
            ("app.js", PlantillasFrontend.AppJs),
            ("README.md", PlantillasFrontend.ReadmeFrontend)
        };

        foreach (var (archivo, plantilla) in archivos)
        {
            escritor.Escribir(
                Path.Combine(destino, archivo),
                GeneradorDeSolucion.Render(plantilla, entidad,
                    ("__PROYECTO_API__", proyectoApi),
                    ("__PROYECTO__", nombre),
                    ("__API_URL__", url)),
                sobrescribir: false);
        }

        Ui.Linea();

        foreach (var creado in escritor.Creados)
        {
            Ui.Ok($"Creado: {creado}");
        }

        foreach (var omitido in escritor.Omitidos)
        {
            Ui.Aviso($"Ya existía (no se modificó): {omitido}");
        }

        if (escritor.Creados.Count == 0)
        {
            return 1;
        }

        Ui.Linea();
        Ui.Nota($"La página consume {url}");
        Ui.Nota("Si la API usa otro puerto, ajustá API_URL en app.js.");

        Ui.ProximosPasos(new[]
        {
            $"dotnet run --project {proyectoApi}",
            $"abrir {Path.Combine(SafePath.ParaMostrar(destino), "index.html")} en el navegador"
        });

        return 0;
    }

    /// <summary>Busca una carpeta de proyecto Web API en el directorio actual.</summary>
    private static string? BuscarProyectoApi()
    {
        try
        {
            return Directory
                .EnumerateDirectories(Directory.GetCurrentDirectory(), "*.Api", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .FirstOrDefault(nombre => !string.IsNullOrEmpty(nombre));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
