using UcseCLI.Core;
using UcseCLI.Templates;

namespace UcseCLI.Generators;

/// <summary>Opciones del armado de una solución.</summary>
public sealed record OpcionesDeSolucion
{
    public required string Nombre { get; init; }

    /// <summary>Entidad del código de ejemplo (por defecto "Producto").</summary>
    public string Entidad { get; init; } = "Producto";

    /// <summary>Target framework. Si es null se usa el default del SDK instalado.</summary>
    public string? Framework { get; init; }

    public bool ConEjemplos { get; init; } = true;

    /// <summary>
    /// Si es false (lo predeterminado), el código de ejemplo se genera comentado
    /// para que borrar cualquier parte no deje al resto del proyecto con errores.
    /// </summary>
    public bool EjemplosActivos { get; init; }

    public bool ConConsola { get; init; } = true;

    public bool ConApi { get; init; }

    public bool ConGit { get; init; } = true;
}

/// <summary>
/// Arma la solución por capas que usa la materia: proyecto de lógica
/// (Entidades / Servicios / Datos), proyecto de tests NUnit y, según el caso,
/// aplicación de consola o Web API.
/// </summary>
public static class GeneradorDeSolucion
{
    public static int Crear(OpcionesDeSolucion opciones)
    {
        var nombre = opciones.Nombre;

        if (!Naming.EsNombreDeProyectoValido(nombre))
        {
            Ui.Error(
                $"Nombre de solución inválido: '{nombre}'.",
                "Usá solo letras, números y guiones bajos, empezando con una letra (por ejemplo: SistemaVentas).");

            return 1;
        }

        var entidad = Naming.APascalCase(opciones.Entidad);

        if (!Naming.EsIdentificadorValido(entidad))
        {
            Ui.Error(
                $"Nombre de entidad inválido: '{opciones.Entidad}'.",
                "Usá un identificador C# válido (por ejemplo: Producto, Ticket, Evento).");

            return 1;
        }

        if (opciones.Framework is not null && !Naming.EsFrameworkValido(opciones.Framework))
        {
            Ui.Error(
                $"Framework inválido: '{opciones.Framework}'.",
                "Se espera un target framework como net8.0, net9.0 o net10.0.");

            return 1;
        }

        var raiz = SafePath.ResolverDentroDelActual(nombre);

        if (raiz is null)
        {
            Ui.Error("La solución tiene que crearse dentro del directorio actual.");
            return 1;
        }

        if (Directory.Exists(raiz) || File.Exists(raiz))
        {
            Ui.Error(
                $"Ya existe '{nombre}' en esta carpeta.",
                "Elegí otro nombre o movete a otro directorio.");

            return 1;
        }

        var logica = $"{nombre}.LogicaServicio";
        var test = $"{nombre}.LogicaTest";
        var consola = $"{nombre}.Consola";
        var api = $"{nombre}.Api";

        Directory.CreateDirectory(raiz);

        var escritor = new EscritorDeArchivos(raiz);

        try
        {
            Ui.Titulo(opciones.ConApi
                ? $"Solución Web API: {nombre}"
                : $"Solución por capas: {nombre}");

            CrearProyectos(raiz, opciones, logica, test, consola, api);
            AgregarALaSolucion(raiz, opciones, logica, test, consola, api);
            AgregarReferencias(raiz, opciones, logica, test, consola, api);

            Ui.Paso($"Agregando {Paquetes.NewtonsoftJson} {Paquetes.VersionNewtonsoftJson} al proyecto de lógica");
            ProyectoCs.AgregarPaquete(
                Path.Combine(raiz, logica, $"{logica}.csproj"),
                Paquetes.NewtonsoftJson,
                Paquetes.VersionNewtonsoftJson);

            CrearCarpetasDeCapas(raiz, logica);

            if (opciones.ConEjemplos)
            {
                Ui.Paso(opciones.EjemplosActivos
                    ? $"Generando código de ejemplo para la entidad {entidad}"
                    : $"Generando código de ejemplo (comentado) para la entidad {entidad}");

                EscribirEjemplosDeLogica(escritor, raiz, logica, test, entidad, opciones.EjemplosActivos);

                if (opciones.ConConsola)
                {
                    EscribirProgramaDeConsola(
                        escritor, raiz, nombre, logica, consola, entidad, opciones.EjemplosActivos);
                }
            }

            if (opciones.ConApi)
            {
                GeneradorDeWebApi.Escribir(
                    escritor, raiz, logica, api, entidad, opciones.ConEjemplos, opciones.EjemplosActivos);
            }

            EscribirArchivosDeRepositorio(escritor, raiz, opciones, logica, test, consola, api, entidad);

            if (opciones.ConGit)
            {
                IniciarRepositorio(raiz);
            }

            MostrarResultado(raiz, escritor, opciones, nombre, logica, test, consola, api);

            return 0;
        }
        catch (DotnetException ex)
        {
            Ui.Error(ex.Message, ex.Detalle);
            Ui.Nota($"La carpeta '{SafePath.ParaMostrar(raiz)}' quedó a medio armar: revisala o borrala antes de reintentar.");

            return 1;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Ui.Error("No se pudo escribir en el disco.", ex.Message);
            return 1;
        }
    }

    private static void CrearProyectos(
        string raiz,
        OpcionesDeSolucion opciones,
        string logica,
        string test,
        string consola,
        string api)
    {
        Ui.Paso("Creando la solución");
        DotnetCli.EjecutarOFallar(raiz, "new", "sln", "-n", opciones.Nombre);

        Ui.Paso($"Creando {logica} (class library)");
        DotnetCli.EjecutarOFallar(raiz, DotnetCli.ConFramework(
            new[] { "new", "classlib", "-n", logica }, opciones.Framework));

        // La plantilla de class library trae una clase vacía que no se usa.
        BorrarArchivoDePlantilla(Path.Combine(raiz, logica, "Class1.cs"));

        Ui.Paso($"Creando {test} (NUnit)");
        DotnetCli.EjecutarOFallar(raiz, DotnetCli.ConFramework(
            new[] { "new", "nunit", "-n", test }, opciones.Framework));

        BorrarArchivoDePlantilla(Path.Combine(raiz, test, "UnitTest1.cs"));
        BorrarArchivoDePlantilla(Path.Combine(raiz, test, "Tests.cs"));

        if (opciones.ConConsola)
        {
            Ui.Paso($"Creando {consola} (aplicación de consola)");
            DotnetCli.EjecutarOFallar(raiz, DotnetCli.ConFramework(
                new[] { "new", "console", "-n", consola }, opciones.Framework));
        }

        if (opciones.ConApi)
        {
            Ui.Paso($"Creando {api} (Web API con controllers)");
            CrearProyectoWebApi(raiz, api, opciones.Framework);
        }
    }

    /// <summary>
    /// Crea el proyecto Web API pidiendo controllers. Desde .NET 8 la plantilla
    /// genera Minimal APIs por defecto, así que sin este flag no habría
    /// carpeta Controllers ni AddControllers() en Program.cs.
    /// </summary>
    private static void CrearProyectoWebApi(string raiz, string api, string? framework)
    {
        var conControllers = DotnetCli.Ejecutar(raiz, DotnetCli.ConFramework(
            new[] { "new", "webapi", "-n", api, "--use-controllers" }, framework));

        if (!conControllers.Exitoso)
        {
            // SDKs viejos no conocen --use-controllers: se arma igual y la
            // configuración por controllers la escribe la herramienta.
            DotnetCli.EjecutarOFallar(raiz, DotnetCli.ConFramework(
                new[] { "new", "webapi", "-n", api }, framework));
        }

        // Ejemplos de la plantilla que se reemplazan por los de la materia.
        BorrarArchivoDePlantilla(Path.Combine(raiz, api, "WeatherForecast.cs"));
        BorrarArchivoDePlantilla(Path.Combine(raiz, api, "Controllers", "WeatherForecastController.cs"));
    }

    private static void AgregarALaSolucion(
        string raiz,
        OpcionesDeSolucion opciones,
        string logica,
        string test,
        string consola,
        string api)
    {
        Ui.Paso("Agregando los proyectos a la solución");

        foreach (var proyecto in ProyectosDe(opciones, logica, test, consola, api))
        {
            DotnetCli.EjecutarOFallar(raiz, "sln", "add", RutaCsproj(proyecto));
        }
    }

    private static void AgregarReferencias(
        string raiz,
        OpcionesDeSolucion opciones,
        string logica,
        string test,
        string consola,
        string api)
    {
        Ui.Paso("Configurando las referencias entre proyectos");

        // Los tests dependen de la lógica, nunca al revés.
        DotnetCli.EjecutarOFallar(raiz, "add", RutaCsproj(test), "reference", RutaCsproj(logica));

        if (opciones.ConConsola)
        {
            // La presentación usa la lógica.
            DotnetCli.EjecutarOFallar(raiz, "add", RutaCsproj(consola), "reference", RutaCsproj(logica));
        }

        if (opciones.ConApi)
        {
            // La API usa la lógica (y no al revés: así la lógica no conoce los DTOs).
            DotnetCli.EjecutarOFallar(raiz, "add", RutaCsproj(api), "reference", RutaCsproj(logica));
        }
    }

    private static void CrearCarpetasDeCapas(string raiz, string logica)
    {
        foreach (var carpeta in new[] { "Entidades", "Servicios", "Datos" })
        {
            Directory.CreateDirectory(Path.Combine(raiz, logica, carpeta));
        }
    }

    private static void EscribirEjemplosDeLogica(
        EscritorDeArchivos escritor,
        string raiz,
        string logica,
        string test,
        string entidad,
        bool activos)
    {
        var nsEntidades = $"{logica}.Entidades";
        var nsServicios = $"{logica}.Servicios";
        var nsDatos = $"{logica}.Datos";

        escritor.Escribir(
            Path.Combine(raiz, logica, "Entidades", $"{entidad}.cs"),
            Plantilla.SegunModo(
                Render(PlantillasDeCodigo.Entidad, entidad,
                    ("__NS__", Plantilla.Namespace(nsEntidades))),
                $"Entidad {entidad} (Unidad 2)",
                activos));

        escritor.Escribir(
            Path.Combine(raiz, logica, "Datos", $"{entidad}Repositorio.cs"),
            Plantilla.SegunModo(
                Render(PlantillasDeCodigo.Repositorio, entidad,
                    ("__NS__", Plantilla.Namespace(nsDatos)),
                    ("__USING_ENTIDADES__", Plantilla.Usings(nsEntidades))),
                $"Repositorio de {entidad} - persistencia en JSON (Unidad 5)",
                activos));

        escritor.Escribir(
            Path.Combine(raiz, logica, "Servicios", $"{entidad}Service.cs"),
            Plantilla.SegunModo(
                Render(PlantillasDeCodigo.Servicio, entidad,
                    ("__NS__", Plantilla.Namespace(nsServicios)),
                    ("__USING_ENTIDADES__", Plantilla.Usings(nsEntidades)),
                    ("__USING_DATOS__", Plantilla.Usings(nsDatos))),
                $"Servicio de {entidad} - reglas de negocio (Unidades 2 y 5)",
                activos));

        escritor.Escribir(
            Path.Combine(raiz, test, $"{entidad}ServiceTest.cs"),
            Plantilla.SegunModo(
                Render(PlantillasDeCodigo.TestDeServicio, entidad,
                    ("__NS__", Plantilla.Namespace(test)),
                    ("__USING_ENTIDADES__", Plantilla.Usings(nsEntidades)),
                    ("__USING_SERVICIOS__", Plantilla.Usings(nsServicios)),
                    ("__USING_DATOS__", Plantilla.Usings(nsDatos))),
                $"Tests de {entidad}Service (Unidad 4)",
                activos));
    }

    private static void EscribirProgramaDeConsola(
        EscritorDeArchivos escritor,
        string raiz,
        string nombre,
        string logica,
        string consola,
        string entidad,
        bool activos)
    {
        var programa = Render(PlantillasProyecto.ProgramaDeConsola, entidad,
            ("__PROYECTO__", nombre),
            ("__USING_ENTIDADES__", Plantilla.Usings($"{logica}.Entidades")),
            ("__USING_SERVICIOS__", Plantilla.Usings($"{logica}.Servicios")));

        // Un proyecto de consola necesita punto de entrada: si el ejemplo va
        // comentado, queda una línea viva al final (y el ejemplo, arriba, para
        // que al descomentarlo los using queden antes de cualquier sentencia).
        var contenido = activos
            ? programa
            : Plantilla.Comentar(
                  programa,
                  "Aplicación de consola - capa de presentación (Unidad 2)",
                  "Al descomentarlo, borrá la línea Console.WriteLine del final del archivo.")
              + Environment.NewLine + Environment.NewLine
              + PlantillasProyecto.ProgramaDeConsolaEnBlanco;

        escritor.Escribir(Path.Combine(raiz, consola, "Program.cs"), contenido);
    }

    private static void EscribirArchivosDeRepositorio(
        EscritorDeArchivos escritor,
        string raiz,
        OpcionesDeSolucion opciones,
        string logica,
        string test,
        string consola,
        string api,
        string entidad)
    {
        escritor.Escribir(Path.Combine(raiz, ".gitignore"), PlantillasProyecto.GitIgnore, sobrescribir: false);

        var proyectos = ProyectosDe(opciones, logica, test, consola, api).ToList();

        var filas = proyectos.Select(proyecto => FilaDelReadme(proyecto, logica, test, consola));

        var comandos = new List<string>();

        if (opciones.ConConsola)
        {
            comandos.Add($"{Environment.NewLine}# ejecutar la aplicación de consola{Environment.NewLine}dotnet run --project {consola}");
        }

        if (opciones.ConApi)
        {
            comandos.Add($"{Environment.NewLine}# levantar la Web API{Environment.NewLine}dotnet run --project {api}");
        }

        var estructura = string.Join(
            Environment.NewLine,
            proyectos.Select(proyecto => proyecto + "/"));

        escritor.Escribir(
            Path.Combine(raiz, "README.md"),
            Render(PlantillasProyecto.Readme, entidad,
                ("__PROYECTO__", opciones.Nombre),
                ("__ESTRUCTURA__", estructura),
                ("__TABLA_PROYECTOS__", string.Join(Environment.NewLine, filas)),
                ("__COMANDOS_EJECUCION__", string.Concat(comandos)),
                ("__NOTA_EJEMPLOS__", NotaDeEjemplos(opciones, entidad))),
            sobrescribir: false);
    }

    /// <summary>Nota del README sobre el código de ejemplo generado.</summary>
    private static string NotaDeEjemplos(OpcionesDeSolucion opciones, string entidad)
    {
        if (!opciones.ConEjemplos)
        {
            return "> La estructura se generó sin código de ejemplo.";
        }

        if (opciones.EjemplosActivos)
        {
            return $"> El código de ejemplo (`{entidad}`) está para tomarlo como referencia:" +
                   Environment.NewLine +
                   "> reemplazalo por el modelo que pide el enunciado.";
        }

        return $"> El código de ejemplo (`{entidad}`) se generó **comentado**, así se puede borrar" +
               Environment.NewLine +
               "> sin que el resto del proyecto quede con errores. Para verlo funcionando," +
               Environment.NewLine +
               "> seleccioná el bloque y descomentalo (`Ctrl+K Ctrl+U` en VS Code)." +
               Environment.NewLine +
               "> Mientras siga comentado, `dotnet test` no encuentra tests para correr.";
    }

    /// <summary>Fila de la tabla del README con la capa y la unidad de cada proyecto.</summary>
    private static string FilaDelReadme(string proyecto, string logica, string test, string consola)
    {
        if (proyecto == logica)
        {
            return $"| `{proyecto}` | Lógica de negocio y datos | 2, 5 |";
        }

        if (proyecto == test)
        {
            return $"| `{proyecto}` | Tests unitarios (NUnit) | 4 |";
        }

        if (proyecto == consola)
        {
            return $"| `{proyecto}` | Presentación (consola) | 1, 2 |";
        }

        return $"| `{proyecto}` | Web API (controllers y DTOs) | 6 |";
    }

    private static void IniciarRepositorio(string raiz)
    {
        if (Directory.Exists(Path.Combine(raiz, ".git")))
        {
            return;
        }

        Ui.Paso("Inicializando el repositorio Git");

        var resultado = Git.Inicializar(raiz);

        if (!resultado)
        {
            Ui.Aviso("No se pudo inicializar Git (¿está instalado?). El resto de la solución quedó creada.");
        }
    }

    private static void MostrarResultado(
        string raiz,
        EscritorDeArchivos escritor,
        OpcionesDeSolucion opciones,
        string nombre,
        string logica,
        string test,
        string consola,
        string api)
    {
        Ui.Linea();
        Ui.Ok($"Solución '{nombre}' creada en {SafePath.ParaMostrar(raiz)}");
        Ui.Linea();

        var archivoSolucion = BuscarArchivoDeSolucion(raiz);
        var rutas = escritor.Creados.ToList();

        if (archivoSolucion is not null)
        {
            rutas.Add(archivoSolucion);
        }

        foreach (var proyecto in ProyectosDe(opciones, logica, test, consola, api))
        {
            rutas.Add($"{proyecto}/{proyecto}.csproj");
        }

        Ui.Estructura(nombre, rutas);

        var pasos = new List<string> { $"cd {nombre}", "dotnet build", "dotnet test" };

        if (opciones.ConConsola)
        {
            pasos.Add($"dotnet run --project {consola}");
        }

        if (opciones.ConApi)
        {
            pasos.Add($"dotnet run --project {api}");
        }

        Ui.ProximosPasos(pasos);

        if (opciones.ConEjemplos)
        {
            Ui.AvisoEjemplos(opciones.EjemplosActivos);
        }
    }

    private static IEnumerable<string> ProyectosDe(
        OpcionesDeSolucion opciones,
        string logica,
        string test,
        string consola,
        string api)
    {
        if (opciones.ConApi)
        {
            yield return api;
        }

        if (opciones.ConConsola)
        {
            yield return consola;
        }

        yield return logica;
        yield return test;
    }

    /// <summary>Nombre del archivo de solución creado (.sln o .slnx, según el SDK).</summary>
    private static string? BuscarArchivoDeSolucion(string raiz)
    {
        var archivo = Directory
            .EnumerateFiles(raiz, "*.sln", SearchOption.TopDirectoryOnly)
            .Concat(Directory.EnumerateFiles(raiz, "*.slnx", SearchOption.TopDirectoryOnly))
            .FirstOrDefault();

        return archivo is null ? null : Path.GetFileName(archivo);
    }

    private static string RutaCsproj(string proyecto) => Path.Combine(proyecto, $"{proyecto}.csproj");

    private static void BorrarArchivoDePlantilla(string ruta)
    {
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }
    }

    /// <summary>
    /// Reemplaza los marcadores comunes de las plantillas. El orden importa:
    /// primero los nombres largos, después los cortos.
    /// </summary>
    internal static string Render(
        string plantilla,
        string entidad,
        params (string Marcador, string Valor)[] extra)
    {
        var marcadores = new List<(string, string)>
        {
            ("__ENTIDAD__", entidad),
            ("__entidades__", Naming.APlural(entidad)),
            ("__entidad__", Naming.ACamelCase(entidad))
        };

        marcadores.AddRange(extra);

        var texto = Plantilla.Render(plantilla, marcadores.ToArray());

        // Los marcadores opcionales que nadie completó se eliminan.
        return LimpiarMarcadoresSinUsar(texto);
    }

    private static string LimpiarMarcadoresSinUsar(string texto)
    {
        foreach (var marcador in new[]
                 {
                     "__USING_ENTIDADES__", "__USING_SERVICIOS__", "__USING_DATOS__",
                     "__USING_DTOS__", "__USINGS__", "__NS__", "__REGISTRO_SERVICIOS__"
                 })
        {
            texto = texto.Replace(marcador, string.Empty, StringComparison.Ordinal);
        }

        return texto;
    }
}
