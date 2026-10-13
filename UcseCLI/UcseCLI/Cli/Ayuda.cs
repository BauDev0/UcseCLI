using System.Reflection;
using UcseCLI.Core;
using UcseCLI.Templates;

namespace UcseCLI.Cli;

/// <summary>
/// Comandos informativos: catálogo de ejemplos reutilizables, .gitignore y
/// estado del entorno.
/// </summary>
public static class Ayuda
{
    public static int MostrarEjemplos(string? tema, string? rutaParaGuardar)
    {
        if (string.IsNullOrWhiteSpace(tema))
        {
            Ui.Titulo("Ejemplos disponibles");

            Ui.Columnas(Ejemplos.Todos.Select(ejemplo =>
                (ejemplo.Clave, $"{ejemplo.Titulo} ({ejemplo.Unidad})")));

            Ui.Linea();
            Ui.Nota("Ver uno:     ucse ejemplos linq");
            Ui.Nota("Guardarlo:   ucse ejemplos linq --guardar Notas/linq.cs");

            return 0;
        }

        var elegido = Ejemplos.Buscar(tema);

        if (elegido is null)
        {
            Ui.Error($"No hay un ejemplo llamado '{tema}'.");
            Ui.Linea();
            Ui.Columnas(Ejemplos.Todos.Select(ejemplo => (ejemplo.Clave, ejemplo.Titulo)));

            return 1;
        }

        if (!string.IsNullOrWhiteSpace(rutaParaGuardar))
        {
            var destino = SafePath.ResolverDentroDelActual(rutaParaGuardar);

            if (destino is null)
            {
                Ui.Error("El archivo tiene que quedar dentro del directorio actual.");
                return 1;
            }

            if (File.Exists(destino))
            {
                Ui.Error($"Ya existe el archivo '{SafePath.ParaMostrar(destino)}'.");
                return 1;
            }

            var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());
            escritor.Escribir(destino, elegido.Codigo);

            Ui.Ok($"Guardado: {SafePath.ParaMostrar(destino)}");
            Ui.Nota("Es un fragmento de referencia: adaptalo a tu modelo antes de usarlo.");

            return 0;
        }

        Ui.Titulo($"{elegido.Titulo} - {elegido.Unidad}");
        Ui.Codigo(elegido.Codigo);
        Ui.Linea();
        Ui.Nota($"Guardarlo en un archivo: ucse ejemplos {elegido.Clave} --guardar <ruta>");

        return 0;
    }

    public static int CrearGitIgnore()
    {
        var destino = Path.Combine(Directory.GetCurrentDirectory(), ".gitignore");

        if (File.Exists(destino))
        {
            Ui.Aviso("Ya existe un .gitignore en esta carpeta: no se modificó.");
            return 1;
        }

        var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());
        escritor.Escribir(destino, PlantillasProyecto.GitIgnore);

        Ui.Ok("Creado: .gitignore");

        return 0;
    }

    public static int MostrarInfo()
    {
        Ui.Titulo($"UCSE CLI {VersionDeLaHerramienta()}");

        Ui.Subtitulo("Entorno");

        var versionSdk = DotnetCli.ObtenerVersion();

        if (versionSdk is null)
        {
            Ui.Error(
                "No se encontró el SDK de .NET.",
                "Instalalo desde https://dotnet.microsoft.com/download (la materia usa .NET 8).");
        }
        else
        {
            Ui.Ok($"SDK de .NET: {versionSdk}");

            var sdks = DotnetCli.ListarSdks();

            if (sdks.Count > 1)
            {
                Ui.Items(sdks);
            }
        }

        if (Git.EsRepositorio(Directory.GetCurrentDirectory()))
        {
            Ui.Ok("Carpeta actual: dentro de un repositorio Git");
        }
        else
        {
            Ui.Nota("Carpeta actual: no es un repositorio Git");
        }

        Ui.Linea();
        Ui.Subtitulo("Armar una solución");
        Ui.Columnas(new[]
        {
            ("ucse new <Nombre>", "Consola + lógica + tests NUnit (Unidades 1 a 5)"),
            ("ucse new-webapi <Nombre>", "Web API + lógica + tests (Unidad 6)"),
            ("ucse new-frontend [carpeta]", "HTML + CSS + JS que consume la API (Unidad 7)"),
            ("ucse add-api", "Sumar una Web API a una solución existente")
        });

        Ui.Linea();
        Ui.Subtitulo("Agregar código");
        Ui.Columnas(new[]
        {
            ("ucse add-clase <ruta> <Nombres...>", "Clases vacías"),
            ("ucse add-entidad <ruta> <Nombres...>", "Entidades del dominio"),
            ("ucse add-interfaz <ruta> <Nombres...>", "Interfaces (prefijo I)"),
            ("ucse add-abstracta <ruta> <Nombres...>", "Clases abstractas"),
            ("ucse add-enum <ruta> <Nombres...>", "Enumeradores"),
            ("ucse add-servicio <ruta> <Entidad>", "Servicio con operaciones básicas"),
            ("ucse add-repositorio <ruta> <Entidad>", "Repositorio con persistencia JSON"),
            ("ucse add-test <ruta> <Clase>", "TestFixture de NUnit (patrón AAA)"),
            ("ucse add-controller <proyecto-api> <Entidad>", "Controller REST con DTOs")
        });

        Ui.Linea();
        Ui.Subtitulo("Referencias");
        Ui.Columnas(new[]
        {
            ("ucse ejemplos", "Fragmentos de código de la materia"),
            ("ucse gitignore", ".gitignore para proyectos .NET"),
            ("ucse <comando> --help", "Opciones de cada comando")
        });

        Ui.Linea();
        Ui.Nota("La herramienta automatiza la estructura y deja código de ejemplo para adaptar.");
        Ui.Nota("El análisis del enunciado y las reglas de negocio siguen siendo tu trabajo.");

        return 0;
    }

    private static string VersionDeLaHerramienta()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(version))
        {
            // El SDK agrega el hash del commit después de un '+'.
            var separador = version.IndexOf('+');

            return separador > 0 ? version[..separador] : version;
        }

        return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "desconocida";
    }
}
