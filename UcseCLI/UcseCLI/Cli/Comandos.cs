using System.CommandLine;
using UcseCLI.Core;
using UcseCLI.Generators;

namespace UcseCLI.Cli;

/// <summary>
/// Definición de los comandos de la herramienta. Cada comando solo lee los
/// argumentos y delega en un generador: la lógica no vive acá.
/// </summary>
public static class Comandos
{
    public static RootCommand CrearRaiz()
    {
        var raiz = new RootCommand(
            "UCSE CLI - Automatiza el armado de proyectos de Programación 1 (UCSE)");

        raiz.Subcommands.Add(NuevaSolucion());
        raiz.Subcommands.Add(NuevaSolucionWebApi());
        raiz.Subcommands.Add(NuevoFrontend());
        raiz.Subcommands.Add(AgregarApi());

        raiz.Subcommands.Add(AgregarArchivos("add-clase", "Crear clases", TipoDeArchivo.Clase));
        raiz.Subcommands.Add(AgregarArchivos("add-files", "Crear clases (alias de add-clase)", TipoDeArchivo.Clase));
        raiz.Subcommands.Add(AgregarArchivos("add-entidad", "Crear entidades del dominio", TipoDeArchivo.Entidad));
        raiz.Subcommands.Add(AgregarArchivos("add-interfaz", "Crear interfaces", TipoDeArchivo.Interfaz));
        raiz.Subcommands.Add(AgregarArchivos("add-abstracta", "Crear clases abstractas", TipoDeArchivo.ClaseAbstracta));
        raiz.Subcommands.Add(AgregarArchivos("add-enum", "Crear enumeradores", TipoDeArchivo.Enumerador));

        raiz.Subcommands.Add(AgregarServicio());
        raiz.Subcommands.Add(AgregarRepositorio());
        raiz.Subcommands.Add(AgregarTest());
        raiz.Subcommands.Add(AgregarControlador());

        raiz.Subcommands.Add(VerEjemplos());
        raiz.Subcommands.Add(CrearGitIgnore());
        raiz.Subcommands.Add(Info());

        // Sin subcomando, la herramienta muestra el estado del entorno y la guía.
        raiz.SetAction(_ =>
        {
            return Ayuda.MostrarInfo();
        });

        return raiz;
    }

    // ---------------------------------------------------------------- soluciones

    private static Command NuevaSolucion()
    {
        var comando = new Command(
            "new",
            "Crear una solución por capas: consola + lógica + tests NUnit");

        var nombre = new Argument<string>("nombre")
        {
            Description = "Nombre de la solución (por ejemplo: SistemaVentas)"
        };

        var entidad = OpcionEntidad();
        var framework = OpcionFramework();
        var sinEjemplos = OpcionSinEjemplos();
        var ejemplosActivos = OpcionEjemplosActivos();
        var sinGit = OpcionSinGit();

        var sinConsola = new Option<bool>("--sin-consola")
        {
            Description = "No crear el proyecto de consola (solo lógica y tests)"
        };

        var conApi = new Option<bool>("--con-api")
        {
            Description = "Agregar además un proyecto Web API"
        };

        comando.Arguments.Add(nombre);
        comando.Options.Add(entidad);
        comando.Options.Add(framework);
        comando.Options.Add(sinEjemplos);
        comando.Options.Add(ejemplosActivos);
        comando.Options.Add(sinConsola);
        comando.Options.Add(conApi);
        comando.Options.Add(sinGit);

        comando.SetAction(resultado =>
        {
            return GeneradorDeSolucion.Crear(new OpcionesDeSolucion
            {
                Nombre = resultado.GetValue(nombre) ?? string.Empty,
                Entidad = resultado.GetValue(entidad) ?? "Producto",
                Framework = resultado.GetValue(framework),
                ConEjemplos = !resultado.GetValue(sinEjemplos),
                EjemplosActivos = resultado.GetValue(ejemplosActivos),
                ConConsola = !resultado.GetValue(sinConsola),
                ConApi = resultado.GetValue(conApi),
                ConGit = !resultado.GetValue(sinGit)
            });
        });

        return comando;
    }

    private static Command NuevaSolucionWebApi()
    {
        var comando = new Command(
            "new-webapi",
            "Crear una solución con Web API: API (controllers y DTOs) + lógica + tests");

        var nombre = new Argument<string>("nombre")
        {
            Description = "Nombre de la solución (por ejemplo: SistemaVentas)"
        };

        var entidad = OpcionEntidad();
        var framework = OpcionFramework();
        var sinEjemplos = OpcionSinEjemplos();
        var ejemplosActivos = OpcionEjemplosActivos();
        var sinGit = OpcionSinGit();

        var conConsola = new Option<bool>("--con-consola")
        {
            Description = "Agregar además un proyecto de consola"
        };

        comando.Arguments.Add(nombre);
        comando.Options.Add(entidad);
        comando.Options.Add(framework);
        comando.Options.Add(sinEjemplos);
        comando.Options.Add(ejemplosActivos);
        comando.Options.Add(conConsola);
        comando.Options.Add(sinGit);

        comando.SetAction(resultado =>
        {
            return GeneradorDeSolucion.Crear(new OpcionesDeSolucion
            {
                Nombre = resultado.GetValue(nombre) ?? string.Empty,
                Entidad = resultado.GetValue(entidad) ?? "Producto",
                Framework = resultado.GetValue(framework),
                ConEjemplos = !resultado.GetValue(sinEjemplos),
                EjemplosActivos = resultado.GetValue(ejemplosActivos),
                ConConsola = resultado.GetValue(conConsola),
                ConApi = true,
                ConGit = !resultado.GetValue(sinGit)
            });
        });

        return comando;
    }

    private static Command NuevoFrontend()
    {
        var comando = new Command(
            "new-frontend",
            "Crear un frontend de ejemplo (HTML + CSS + JavaScript) que consume la API");

        var carpeta = new Argument<string>("carpeta")
        {
            Description = "Carpeta destino (por defecto: Frontend)",
            Arity = ArgumentArity.ZeroOrOne
        };

        var api = new Option<string>("--api")
        {
            Description = "URL del endpoint (por defecto: http://localhost:5000/api/<entidad>)"
        };

        var entidad = OpcionEntidad();

        comando.Arguments.Add(carpeta);
        comando.Options.Add(api);
        comando.Options.Add(entidad);

        comando.SetAction(resultado =>
        {
            return GeneradorDeFrontend.Crear(
                resultado.GetValue(carpeta) ?? "Frontend",
                resultado.GetValue(api),
                resultado.GetValue(entidad) ?? "Producto");
        });

        return comando;
    }

    private static Command AgregarApi()
    {
        var comando = new Command(
            "add-api",
            "Agregar un proyecto Web API a una solución existente (se corre en la raíz de la solución)");

        var entidad = new Option<string>("--entidad", "-e")
        {
            Description = "Entidad del controller de ejemplo (si no se indica, se detecta de la lógica)"
        };

        var logica = OpcionLogica();
        var framework = OpcionFramework();
        var ejemplosActivos = OpcionEjemplosActivos();

        comando.Options.Add(entidad);
        comando.Options.Add(logica);
        comando.Options.Add(framework);
        comando.Options.Add(ejemplosActivos);

        comando.SetAction(resultado =>
        {
            return GeneradorDeApi.Agregar(
                resultado.GetValue(entidad),
                resultado.GetValue(framework),
                resultado.GetValue(logica),
                resultado.GetValue(ejemplosActivos));
        });

        return comando;
    }

    // ------------------------------------------------------------------ archivos

    private static Command AgregarArchivos(string nombreComando, string descripcion, TipoDeArchivo tipo)
    {
        var comando = new Command(nombreComando, descripcion);

        var ruta = new Argument<string>("ruta")
        {
            Description = "Carpeta destino (por ejemplo: Ventas.LogicaServicio/Entidades)"
        };

        var nombres = new Argument<string[]>("nombres")
        {
            Description = "Uno o más nombres",
            Arity = ArgumentArity.OneOrMore
        };

        var espacioDeNombres = OpcionNamespace();

        comando.Arguments.Add(ruta);
        comando.Arguments.Add(nombres);
        comando.Options.Add(espacioDeNombres);

        comando.SetAction(resultado =>
        {
            return GeneradorDeArchivos.Crear(
                resultado.GetValue(ruta) ?? string.Empty,
                resultado.GetValue(nombres) ?? Array.Empty<string>(),
                tipo,
                resultado.GetValue(espacioDeNombres));
        });

        return comando;
    }

    private static Command AgregarServicio()
    {
        var comando = new Command(
            "add-servicio",
            "Crear un servicio de ejemplo con las operaciones básicas sobre una entidad");

        var ruta = new Argument<string>("ruta")
        {
            Description = "Carpeta destino (por ejemplo: Ventas.LogicaServicio/Servicios)"
        };

        var entidad = new Argument<string>("entidad")
        {
            Description = "Entidad sobre la que trabaja el servicio (por ejemplo: Cliente)"
        };

        var espacioDeNombres = OpcionNamespace();
        var ejemplosActivos = OpcionEjemplosActivos();

        comando.Arguments.Add(ruta);
        comando.Arguments.Add(entidad);
        comando.Options.Add(espacioDeNombres);
        comando.Options.Add(ejemplosActivos);

        comando.SetAction(resultado =>
        {
            return GeneradorDeArchivos.CrearServicio(
                resultado.GetValue(ruta) ?? string.Empty,
                resultado.GetValue(entidad) ?? string.Empty,
                resultado.GetValue(espacioDeNombres),
                resultado.GetValue(ejemplosActivos));
        });

        return comando;
    }

    private static Command AgregarRepositorio()
    {
        var comando = new Command(
            "add-repositorio",
            "Crear un repositorio de ejemplo con persistencia en JSON");

        var ruta = new Argument<string>("ruta")
        {
            Description = "Carpeta destino (por ejemplo: Ventas.LogicaServicio/Datos)"
        };

        var entidad = new Argument<string>("entidad")
        {
            Description = "Entidad que persiste el repositorio (por ejemplo: Cliente)"
        };

        var espacioDeNombres = OpcionNamespace();
        var ejemplosActivos = OpcionEjemplosActivos();

        comando.Arguments.Add(ruta);
        comando.Arguments.Add(entidad);
        comando.Options.Add(espacioDeNombres);
        comando.Options.Add(ejemplosActivos);

        comando.SetAction(resultado =>
        {
            return GeneradorDeArchivos.CrearRepositorio(
                resultado.GetValue(ruta) ?? string.Empty,
                resultado.GetValue(entidad) ?? string.Empty,
                resultado.GetValue(espacioDeNombres),
                resultado.GetValue(ejemplosActivos));
        });

        return comando;
    }

    private static Command AgregarTest()
    {
        var comando = new Command(
            "add-test",
            "Crear un TestFixture de NUnit con el patrón Arrange-Act-Assert");

        var ruta = new Argument<string>("ruta")
        {
            Description = "Carpeta destino (por ejemplo: Ventas.LogicaTest)"
        };

        var clase = new Argument<string>("clase")
        {
            Description = "Clase bajo prueba (si termina en Service se generan tests completos)"
        };

        var espacioDeNombres = OpcionNamespace();
        var ejemplosActivos = OpcionEjemplosActivos();

        comando.Arguments.Add(ruta);
        comando.Arguments.Add(clase);
        comando.Options.Add(espacioDeNombres);
        comando.Options.Add(ejemplosActivos);

        comando.SetAction(resultado =>
        {
            return GeneradorDeArchivos.CrearTest(
                resultado.GetValue(ruta) ?? string.Empty,
                resultado.GetValue(clase) ?? string.Empty,
                resultado.GetValue(espacioDeNombres),
                resultado.GetValue(ejemplosActivos));
        });

        return comando;
    }

    private static Command AgregarControlador()
    {
        var comando = new Command(
            "add-controller",
            "Crear un controller REST con sus DTOs (Request, Response y Mapper)");

        var proyecto = new Argument<string>("proyecto-api")
        {
            Description = "Carpeta del proyecto Web API (por ejemplo: Ventas.Api)"
        };

        var entidad = new Argument<string>("entidad")
        {
            Description = "Entidad que expone el controller (por ejemplo: Cliente)"
        };

        var logica = OpcionLogica();
        var ejemplosActivos = OpcionEjemplosActivos();

        comando.Arguments.Add(proyecto);
        comando.Arguments.Add(entidad);
        comando.Options.Add(logica);
        comando.Options.Add(ejemplosActivos);

        comando.SetAction(resultado =>
        {
            return GeneradorDeArchivos.CrearControlador(
                resultado.GetValue(proyecto) ?? string.Empty,
                resultado.GetValue(entidad) ?? string.Empty,
                resultado.GetValue(logica),
                resultado.GetValue(ejemplosActivos));
        });

        return comando;
    }

    // -------------------------------------------------------------------- ayudas

    private static Command VerEjemplos()
    {
        var comando = new Command(
            "ejemplos",
            "Ver fragmentos de código de referencia de la materia, listos para copiar");

        var tema = new Argument<string>("tema")
        {
            Description = "Tema a mostrar (sin argumento se listan todos)",
            Arity = ArgumentArity.ZeroOrOne
        };

        var guardar = new Option<string>("--guardar")
        {
            Description = "Guardar el fragmento en un archivo en lugar de mostrarlo"
        };

        comando.Arguments.Add(tema);
        comando.Options.Add(guardar);

        comando.SetAction(resultado =>
        {
            return Ayuda.MostrarEjemplos(
                resultado.GetValue(tema),
                resultado.GetValue(guardar));
        });

        return comando;
    }

    private static Command CrearGitIgnore()
    {
        var comando = new Command(
            "gitignore",
            "Crear un .gitignore para proyectos .NET en la carpeta actual");

        comando.SetAction(_ =>
        {
            return Ayuda.CrearGitIgnore();
        });

        return comando;
    }

    private static Command Info()
    {
        var comando = new Command(
            "info",
            "Mostrar el estado del entorno (SDK de .NET, Git) y los comandos disponibles");

        comando.SetAction(_ =>
        {
            return Ayuda.MostrarInfo();
        });

        return comando;
    }

    // ------------------------------------------------------------------ opciones

    private static Option<string> OpcionEntidad() =>
        new("--entidad", "-e")
        {
            Description = "Entidad del código de ejemplo (por defecto: Producto)"
        };

    private static Option<string> OpcionFramework() =>
        new("--framework", "-f")
        {
            Description = "Target framework de los proyectos (por ejemplo net8.0). Por defecto, el del SDK instalado"
        };

    private static Option<string> OpcionNamespace() =>
        new("--namespace", "-n")
        {
            Description = "Namespace a usar. Por defecto se deduce del .csproj y de la carpeta"
        };

    private static Option<string> OpcionLogica() =>
        new("--logica")
        {
            Description = "Nombre del proyecto de lógica (si no se puede detectar automáticamente)"
        };

    private static Option<bool> OpcionEjemplosActivos() =>
        new("--ejemplos-activos")
        {
            Description = "Generar el código de ejemplo sin comentar (compila y los tests corren)"
        };

    private static Option<bool> OpcionSinEjemplos() =>
        new("--sin-ejemplos")
        {
            Description = "Generar la estructura sin código de ejemplo"
        };

    private static Option<bool> OpcionSinGit() =>
        new("--sin-git")
        {
            Description = "No inicializar un repositorio Git"
        };
}
