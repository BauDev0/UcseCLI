using UcseCLI.Core;
using UcseCLI.Templates;

namespace UcseCLI.Generators;

/// <summary>Tipo de archivo .cs a generar con los comandos "add-".</summary>
public enum TipoDeArchivo
{
    Clase,
    Interfaz,
    ClaseAbstracta,
    Enumerador,
    Entidad
}

/// <summary>
/// Genera archivos .cs sueltos dentro de un proyecto existente, deduciendo el
/// namespace a partir del .csproj y de la carpeta destino.
/// </summary>
public static class GeneradorDeArchivos
{
    /// <summary>Clases, interfaces, clases abstractas, enums o entidades.</summary>
    public static int Crear(
        string rutaRelativa,
        string[] nombres,
        TipoDeArchivo tipo,
        string? namespaceManual)
    {
        var destino = SafePath.ResolverDentroDelActual(rutaRelativa);

        if (destino is null)
        {
            Ui.Error(
                "La carpeta destino tiene que estar dentro del directorio actual.",
                $"Ruta recibida: {rutaRelativa}");

            return 1;
        }

        if (nombres.Length == 0)
        {
            Ui.Error("Hay que indicar al menos un nombre.");
            return 1;
        }

        // Se valida todo antes de escribir, para no dejar la mitad de los archivos creados.
        var definitivos = new List<string>();

        foreach (var nombre in nombres)
        {
            var nombreFinal = tipo == TipoDeArchivo.Interfaz
                ? Naming.ANombreDeInterfaz(nombre)
                : Naming.APascalCase(nombre);

            if (!Naming.EsIdentificadorValido(nombreFinal))
            {
                Ui.Error(
                    $"Nombre inválido: '{nombre}'.",
                    "Usá letras, números y guiones bajos, sin empezar con un número ni usar palabras reservadas de C#.");

                return 1;
            }

            definitivos.Add(nombreFinal);
        }

        if (namespaceManual is not null && !EsNamespaceValido(namespaceManual))
        {
            Ui.Error($"Namespace inválido: '{namespaceManual}'.");
            return 1;
        }

        var contexto = ProyectoCs.ObtenerContexto(destino);
        var espacioDeNombres = namespaceManual ?? contexto?.NamespaceCarpeta;

        Directory.CreateDirectory(destino);

        var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());

        foreach (var nombre in definitivos)
        {
            var contenido = GeneradorDeSolucion.Render(
                PlantillaDe(tipo),
                nombre,
                ("__NOMBRE__", nombre),
                ("__NS__", Plantilla.Namespace(espacioDeNombres)));

            escritor.Escribir(Path.Combine(destino, $"{nombre}.cs"), contenido, sobrescribir: false);
        }

        return InformarResultado(escritor, espacioDeNombres);
    }

    /// <summary>Servicio de ejemplo con las operaciones básicas sobre una entidad.</summary>
    public static int CrearServicio(
        string rutaRelativa,
        string entidadPedida,
        string? namespaceManual,
        bool ejemplosActivos)
    {
        var preparado = Preparar(rutaRelativa, entidadPedida, namespaceManual);

        if (preparado is null)
        {
            return 1;
        }

        var (destino, entidad, espacioDeNombres, raizDelProyecto) = preparado.Value;
        var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());

        escritor.Escribir(
            Path.Combine(destino, $"{entidad}Service.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasDeCodigo.Servicio, entidad,
                    ("__NS__", Plantilla.Namespace(espacioDeNombres)),
                    ("__USING_ENTIDADES__", Usings(raizDelProyecto, "Entidades", espacioDeNombres)),
                    ("__USING_DATOS__", Usings(raizDelProyecto, "Datos", espacioDeNombres))),
                $"Servicio de {entidad} - reglas de negocio (Unidades 2 y 5)",
                ejemplosActivos),
            sobrescribir: false);

        return InformarResultado(escritor, espacioDeNombres, !ejemplosActivos);
    }

    /// <summary>Repositorio JSON de ejemplo (capa de datos de la Unidad 5).</summary>
    public static int CrearRepositorio(
        string rutaRelativa,
        string entidadPedida,
        string? namespaceManual,
        bool ejemplosActivos)
    {
        var preparado = Preparar(rutaRelativa, entidadPedida, namespaceManual);

        if (preparado is null)
        {
            return 1;
        }

        var (destino, entidad, espacioDeNombres, raizDelProyecto) = preparado.Value;
        var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());

        escritor.Escribir(
            Path.Combine(destino, $"{entidad}Repositorio.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasDeCodigo.Repositorio, entidad,
                    ("__NS__", Plantilla.Namespace(espacioDeNombres)),
                    ("__USING_ENTIDADES__", Usings(raizDelProyecto, "Entidades", espacioDeNombres))),
                $"Repositorio de {entidad} - persistencia en JSON (Unidad 5)",
                ejemplosActivos),
            sobrescribir: false);

        var contexto = ProyectoCs.ObtenerContexto(destino);

        if (contexto is not null && !ProyectoCs.TienePaquete(contexto.RutaCsproj, Paquetes.NewtonsoftJson))
        {
            ProyectoCs.AgregarPaquete(
                contexto.RutaCsproj,
                Paquetes.NewtonsoftJson,
                Paquetes.VersionNewtonsoftJson);

            Ui.Ok($"Agregado {Paquetes.NewtonsoftJson} a {Path.GetFileName(contexto.RutaCsproj)}");
        }

        return InformarResultado(escritor, espacioDeNombres, !ejemplosActivos);
    }

    /// <summary>
    /// Tests NUnit. Si el nombre termina en "Service" genera la batería completa
    /// de ejemplo sobre un archivo temporal; si no, un TestFixture con un test guía.
    /// </summary>
    public static int CrearTest(
        string rutaRelativa,
        string clasePedida,
        string? namespaceManual,
        bool ejemplosActivos)
    {
        var destino = SafePath.ResolverDentroDelActual(rutaRelativa);

        if (destino is null)
        {
            Ui.Error("La carpeta destino tiene que estar dentro del directorio actual.");
            return 1;
        }

        var clase = Naming.APascalCase(clasePedida);

        if (!Naming.EsIdentificadorValido(clase))
        {
            Ui.Error($"Nombre inválido: '{clasePedida}'.");
            return 1;
        }

        if (namespaceManual is not null && !EsNamespaceValido(namespaceManual))
        {
            Ui.Error($"Namespace inválido: '{namespaceManual}'.");
            return 1;
        }

        Directory.CreateDirectory(destino);

        var contexto = ProyectoCs.ObtenerContexto(destino);
        var espacioDeNombres = namespaceManual ?? contexto?.NamespaceCarpeta;
        var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());

        if (clase.EndsWith("Service", StringComparison.Ordinal) && clase.Length > "Service".Length)
        {
            // Tests del servicio: hace falta saber dónde viven la entidad y el repositorio.
            var entidad = clase[..^"Service".Length];
            var logica = ProyectoDeLogica(contexto);

            escritor.Escribir(
                Path.Combine(destino, $"{clase}Test.cs"),
                Plantilla.SegunModo(
                    GeneradorDeSolucion.Render(PlantillasDeCodigo.TestDeServicio, entidad,
                        ("__NS__", Plantilla.Namespace(espacioDeNombres)),
                        ("__USING_ENTIDADES__", Plantilla.Usings(SubNamespace(logica, "Entidades"))),
                        ("__USING_SERVICIOS__", Plantilla.Usings(SubNamespace(logica, "Servicios"))),
                        ("__USING_DATOS__", Plantilla.Usings(SubNamespace(logica, "Datos")))),
                    $"Tests de {clase} (Unidad 4)",
                    ejemplosActivos),
                sobrescribir: false);

            if (logica is null)
            {
                Ui.Aviso("No se detectó el proyecto de lógica: revisá las directivas using del test.");
            }

            return InformarResultado(escritor, espacioDeNombres, !ejemplosActivos);
        }
        else
        {
            var logica = ProyectoDeLogica(contexto);

            escritor.Escribir(
                Path.Combine(destino, $"{clase}Test.cs"),
                GeneradorDeSolucion.Render(PlantillasDeCodigo.TestDeClase, clase,
                    ("__NOMBRE__", clase),
                    ("__NS__", Plantilla.Namespace(espacioDeNombres)),
                    // Van comentados: esas carpetas pueden no tener todavía
                    // código activo, y un using a un namespace inexistente no compila.
                    ("__USINGS__", Plantilla.ComentarBloque(Plantilla.Usings(
                        SubNamespace(logica, "Entidades"),
                        SubNamespace(logica, "Servicios"))))),
                sobrescribir: false);
        }

        return InformarResultado(escritor, espacioDeNombres);
    }

    /// <summary>Controller REST con sus DTOs y mapper dentro de un proyecto Web API.</summary>
    public static int CrearControlador(
        string rutaProyectoApi,
        string entidadPedida,
        string? logicaManual,
        bool ejemplosActivos)
    {
        var carpetaApi = SafePath.ResolverDentroDelActual(rutaProyectoApi);

        if (carpetaApi is null)
        {
            Ui.Error("La carpeta del proyecto Web API tiene que estar dentro del directorio actual.");
            return 1;
        }

        var entidad = Naming.APascalCase(entidadPedida);

        if (!Naming.EsIdentificadorValido(entidad))
        {
            Ui.Error($"Nombre de entidad inválido: '{entidadPedida}'.");
            return 1;
        }

        var contexto = ProyectoCs.ObtenerContexto(carpetaApi);

        if (contexto is null)
        {
            Ui.Error(
                "No se encontró un proyecto (.csproj) en esa carpeta.",
                "Indicá la carpeta del proyecto Web API, por ejemplo: ucse add-controller Ventas.Api Cliente");

            return 1;
        }

        var logica = logicaManual ?? ProyectoDeLogica(contexto);

        if (logica is null)
        {
            Ui.Error(
                "No se pudo determinar el proyecto de lógica referenciado por la API.",
                "Agregá la referencia (dotnet add ... reference ...) o pasá --logica <NombreDelProyecto>.");

            return 1;
        }

        var api = contexto.NamespaceRaiz;
        var nsControllers = $"{api}.Controllers";
        var nsDtos = $"{api}.DTOs";
        var escritor = new EscritorDeArchivos(Directory.GetCurrentDirectory());

        escritor.Escribir(
            Path.Combine(carpetaApi, "Controllers", $"{entidad}Controller.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.Controlador, entidad,
                    ("__NS__", Plantilla.Namespace(nsControllers)),
                    ("__USING_DTOS__", Plantilla.Usings(nsDtos)),
                    ("__USING_ENTIDADES__", Plantilla.Usings($"{logica}.Entidades")),
                    ("__USING_SERVICIOS__", Plantilla.Usings($"{logica}.Servicios"))),
                $"Controller REST de {entidad} (Unidad 6)",
                ejemplosActivos),
            sobrescribir: false);

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Request.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.DtoRequest, entidad,
                    ("__NS__", Plantilla.Namespace(nsDtos))),
                $"DTO de entrada de {entidad} (Unidad 6)",
                ejemplosActivos),
            sobrescribir: false);

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Response.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.DtoResponse, entidad,
                    ("__NS__", Plantilla.Namespace(nsDtos))),
                $"DTO de salida de {entidad} (Unidad 6)",
                ejemplosActivos),
            sobrescribir: false);

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Mapper.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.Mapper, entidad,
                    ("__NS__", Plantilla.Namespace(nsDtos)),
                    ("__USING_ENTIDADES__", Plantilla.Usings($"{logica}.Entidades"))),
                $"Mapeo entre {entidad} y sus DTOs (Unidad 6)",
                ejemplosActivos),
            sobrescribir: false);

        var resultado = InformarResultado(escritor, nsDtos, !ejemplosActivos);

        Ui.Linea();
        Ui.Nota($"Falta registrar el servicio en Program.cs: builder.Services.AddSingleton<{entidad}Service>();");

        return resultado;
    }

    private static (string Destino, string Entidad, string? Namespace, string? RaizProyecto)? Preparar(
        string rutaRelativa,
        string entidadPedida,
        string? namespaceManual)
    {
        var destino = SafePath.ResolverDentroDelActual(rutaRelativa);

        if (destino is null)
        {
            Ui.Error("La carpeta destino tiene que estar dentro del directorio actual.");
            return null;
        }

        var entidad = Naming.APascalCase(entidadPedida);

        if (!Naming.EsIdentificadorValido(entidad))
        {
            Ui.Error($"Nombre de entidad inválido: '{entidadPedida}'.");
            return null;
        }

        if (namespaceManual is not null && !EsNamespaceValido(namespaceManual))
        {
            Ui.Error($"Namespace inválido: '{namespaceManual}'.");
            return null;
        }

        Directory.CreateDirectory(destino);

        var contexto = ProyectoCs.ObtenerContexto(destino);

        if (contexto is null && namespaceManual is null)
        {
            Ui.Aviso("No se encontró un .csproj: los archivos se generan sin namespace.");
        }

        return (destino, entidad, namespaceManual ?? contexto?.NamespaceCarpeta, contexto?.NamespaceRaiz);
    }

    /// <summary>
    /// Namespace de una carpeta hermana dentro del mismo proyecto
    /// (por ejemplo "Ventas.LogicaServicio.Entidades" desde "Servicios").
    /// </summary>
    private static string Usings(string? raizDelProyecto, string carpeta, string? namespaceActual)
    {
        if (raizDelProyecto is null)
        {
            return string.Empty;
        }

        var candidato = $"{raizDelProyecto}.{carpeta}";

        // Si el archivo se genera en esa misma carpeta, el using es innecesario.
        return string.Equals(candidato, namespaceActual, StringComparison.Ordinal)
            ? string.Empty
            : Plantilla.Usings(candidato);
    }

    /// <summary>Namespace de una capa dentro del proyecto de lógica, o null si no se detectó.</summary>
    private static string? SubNamespace(string? proyecto, string capa) =>
        string.IsNullOrWhiteSpace(proyecto) ? null : $"{proyecto}.{capa}";

    /// <summary>Proyecto de lógica referenciado, o el propio proyecto si no hay referencias.</summary>
    private static string? ProyectoDeLogica(ContextoDeProyecto? contexto)
    {
        if (contexto is null)
        {
            return null;
        }

        var referencias = ProyectoCs.LeerProyectosReferenciados(contexto.RutaCsproj);

        if (referencias.Count > 0)
        {
            // Si hay varias, se prefiere la que sigue la convención de la materia.
            return referencias.FirstOrDefault(nombre =>
                       nombre.EndsWith("LogicaServicio", StringComparison.OrdinalIgnoreCase)
                       || nombre.EndsWith("Logica", StringComparison.OrdinalIgnoreCase))
                   ?? referencias[0];
        }

        return contexto.NamespaceRaiz;
    }

    private static string PlantillaDe(TipoDeArchivo tipo) => tipo switch
    {
        TipoDeArchivo.Interfaz => PlantillasDeCodigo.Interfaz,
        TipoDeArchivo.ClaseAbstracta => PlantillasDeCodigo.ClaseAbstracta,
        TipoDeArchivo.Enumerador => PlantillasDeCodigo.Enumerador,
        TipoDeArchivo.Entidad => PlantillasDeCodigo.Entidad,
        _ => PlantillasDeCodigo.Clase
    };

    private static bool EsNamespaceValido(string espacioDeNombres) =>
        espacioDeNombres.Split('.').All(Naming.EsIdentificadorValido);

    private static int InformarResultado(
        EscritorDeArchivos escritor,
        string? espacioDeNombres,
        bool comentado = false)
    {
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

        if (!string.IsNullOrWhiteSpace(espacioDeNombres))
        {
            Ui.Nota($"namespace {espacioDeNombres}");
        }

        if (comentado)
        {
            Ui.AvisoArchivoComentado();
        }

        return 0;
    }
}
