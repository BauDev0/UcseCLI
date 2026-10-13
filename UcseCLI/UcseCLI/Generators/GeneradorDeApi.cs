using UcseCLI.Core;

namespace UcseCLI.Generators;

/// <summary>
/// Suma un proyecto Web API a una solución que ya existe, sin tocar la lógica
/// ni los tests: es el paso de la práctica de la Unidad 6 sobre una solución
/// armada en las Unidades 4 y 5.
/// </summary>
public static class GeneradorDeApi
{
    public static int Agregar(string? entidadPedida, string? framework, string? logicaManual)
    {
        var raiz = Directory.GetCurrentDirectory();
        var solucion = BuscarArchivoDeSolucion(raiz);

        if (solucion is null)
        {
            Ui.Error(
                "No se encontró un archivo de solución en esta carpeta.",
                "Ejecutá el comando desde la carpeta que contiene el .sln (la raíz de la solución).");

            return 1;
        }

        if (framework is not null && !Naming.EsFrameworkValido(framework))
        {
            Ui.Error($"Framework inválido: '{framework}'.", "Se espera algo como net8.0 o net10.0.");
            return 1;
        }

        var logica = logicaManual ?? BuscarProyectoDeLogica(raiz);

        if (logica is null)
        {
            Ui.Error(
                "No se encontró el proyecto de lógica.",
                "Pasalo con --logica <NombreDelProyecto> (la carpeta que contiene el .csproj de la lógica).");

            return 1;
        }

        if (!File.Exists(Path.Combine(raiz, logica, $"{logica}.csproj")))
        {
            Ui.Error($"No existe el proyecto '{logica}' en esta carpeta.");
            return 1;
        }

        var nombre = QuitarSufijo(logica);
        var api = $"{nombre}.Api";

        if (Directory.Exists(Path.Combine(raiz, api)))
        {
            Ui.Error($"Ya existe la carpeta '{api}'.");
            return 1;
        }

        var entidad = entidadPedida is null
            ? DetectarEntidad(Path.Combine(raiz, logica))
            : Naming.APascalCase(entidadPedida);

        if (entidad is not null && !Naming.EsIdentificadorValido(entidad))
        {
            Ui.Error($"Nombre de entidad inválido: '{entidadPedida}'.");
            return 1;
        }

        var conEjemplos = entidad is not null;
        var escritor = new EscritorDeArchivos(raiz);

        try
        {
            Ui.Titulo($"Agregando {api} a {solucion}");

            Ui.Paso($"Creando {api} (Web API con controllers)");

            var conControllers = DotnetCli.Ejecutar(raiz, DotnetCli.ConFramework(
                new[] { "new", "webapi", "-n", api, "--use-controllers" }, framework));

            if (!conControllers.Exitoso)
            {
                DotnetCli.EjecutarOFallar(raiz, DotnetCli.ConFramework(
                    new[] { "new", "webapi", "-n", api }, framework));
            }

            BorrarSiExiste(Path.Combine(raiz, api, "WeatherForecast.cs"));
            BorrarSiExiste(Path.Combine(raiz, api, "Controllers", "WeatherForecastController.cs"));

            Ui.Paso("Agregando el proyecto a la solución");
            DotnetCli.EjecutarOFallar(raiz, "sln", "add", Path.Combine(api, $"{api}.csproj"));

            Ui.Paso($"Referenciando {logica} desde {api}");
            DotnetCli.EjecutarOFallar(
                raiz,
                "add", Path.Combine(api, $"{api}.csproj"),
                "reference", Path.Combine(logica, $"{logica}.csproj"));

            GeneradorDeWebApi.Escribir(escritor, raiz, logica, api, entidad ?? "Producto", conEjemplos);

            Ui.Linea();
            Ui.Ok($"Proyecto '{api}' agregado a la solución");

            if (!conEjemplos)
            {
                Ui.Linea();
                Ui.Aviso("No se detectó un servicio en la lógica, así que no se generó el controller.");
                Ui.Nota($"Cuando tengas el servicio: ucse add-controller {api} <Entidad>");
            }

            Ui.Estructura(api, escritor.Creados
                .Where(ruta => ruta.StartsWith(api + "/", StringComparison.Ordinal))
                .Select(ruta => ruta[(api.Length + 1)..]));

            Ui.ProximosPasos(new[] { "dotnet build", $"dotnet run --project {api}" });

            return 0;
        }
        catch (DotnetException ex)
        {
            Ui.Error(ex.Message, ex.Detalle);
            return 1;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Ui.Error("No se pudo escribir en el disco.", ex.Message);
            return 1;
        }
    }

    private static string? BuscarArchivoDeSolucion(string raiz)
    {
        var archivo = Directory
            .EnumerateFiles(raiz, "*.sln", SearchOption.TopDirectoryOnly)
            .Concat(Directory.EnumerateFiles(raiz, "*.slnx", SearchOption.TopDirectoryOnly))
            .FirstOrDefault();

        return archivo is null ? null : Path.GetFileName(archivo);
    }

    /// <summary>Carpeta de proyecto cuyo nombre sigue la convención de la materia.</summary>
    private static string? BuscarProyectoDeLogica(string raiz)
    {
        var candidatas = Directory
            .EnumerateDirectories(raiz, "*", SearchOption.TopDirectoryOnly)
            .Select(ruta => Path.GetFileName(ruta)!)
            .Where(nombre => File.Exists(Path.Combine(raiz, nombre, $"{nombre}.csproj")))
            .ToList();

        return candidatas.FirstOrDefault(nombre =>
                   nombre.EndsWith(".LogicaServicio", StringComparison.OrdinalIgnoreCase))
               ?? candidatas.FirstOrDefault(nombre =>
                   nombre.EndsWith(".Logica", StringComparison.OrdinalIgnoreCase)
                   || nombre.EndsWith("Logica", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Entidad a usar en el ejemplo: la primera que tenga su Service en la lógica.</summary>
    private static string? DetectarEntidad(string carpetaLogica)
    {
        var servicios = Path.Combine(carpetaLogica, "Servicios");

        if (!Directory.Exists(servicios))
        {
            return null;
        }

        foreach (var archivo in Directory
                     .EnumerateFiles(servicios, "*Service.cs", SearchOption.TopDirectoryOnly)
                     .OrderBy(ruta => ruta, StringComparer.Ordinal))
        {
            var nombre = Path.GetFileNameWithoutExtension(archivo);
            var entidad = nombre[..^"Service".Length];

            if (Naming.EsIdentificadorValido(entidad))
            {
                return entidad;
            }
        }

        return null;
    }

    private static string QuitarSufijo(string proyectoDeLogica)
    {
        foreach (var sufijo in new[] { ".LogicaServicio", ".Logica" })
        {
            if (proyectoDeLogica.EndsWith(sufijo, StringComparison.OrdinalIgnoreCase))
            {
                return proyectoDeLogica[..^sufijo.Length];
            }
        }

        return proyectoDeLogica;
    }

    private static void BorrarSiExiste(string ruta)
    {
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }
    }
}
