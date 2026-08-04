using Spectre.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UcseCLI;

public static class ProjectGenerator
{
    // Solo letras, números y guiones bajos. Debe empezar con letra.
    private static readonly Regex ValidName = new(@"^[a-zA-Z][a-zA-Z0-9_]*$");

    public static void CreateSolution(
        string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName) || !ValidName.IsMatch(projectName))
        {
            AnsiConsole.MarkupLine(
                "[red]Nombre inválido. Usá solo letras, números y guiones bajos, empezando con una letra.[/]"
            );
            return;
        }

        var root = Path.GetFullPath(
            Path.Combine(
                Environment.CurrentDirectory,
                projectName
            )
        );

        if (Directory.Exists(root))
        {
            AnsiConsole.MarkupLine(
                "[red]La carpeta ya existe[/]"
            );

            return;
        }

        Directory.CreateDirectory(root);

        Execute($"new classlib -n {projectName}.LogicaServicio", root);
        Execute($"new nunit -n {projectName}.LogicaTest", root);
        Execute($"new sln -n SolucionGeneral", root);
        Execute($"sln add \"{projectName}.LogicaServicio/{projectName}.LogicaServicio.csproj\"", root);
        Execute($"sln add \"{projectName}.LogicaTest/{projectName}.LogicaTest.csproj\"", root);
        Execute(
            $"add \"{projectName}.LogicaTest/{projectName}.LogicaTest.csproj\" reference \"{projectName}.LogicaServicio/{projectName}.LogicaServicio.csproj\"",
            root
        );

        AnsiConsole.MarkupLine(
            $"[green]Proyecto '{projectName}' creado correctamente[/]"
        );
    }

    public static void CreateWebApiSolution(
        string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName) || !ValidName.IsMatch(projectName))
        {
            AnsiConsole.MarkupLine(
                "[red]Nombre inválido. Usá solo letras, números y guiones bajos, empezando con una letra.[/]"
            );
            return;
        }

        var root = Path.GetFullPath(
            Path.Combine(
                Environment.CurrentDirectory,
                projectName
            )
        );

        if (Directory.Exists(root))
        {
            AnsiConsole.MarkupLine(
                "[red]La carpeta ya existe[/]"
            );

            return;
        }

        Directory.CreateDirectory(root);

        Execute($"new webapi -n {projectName}.Api", root);
        Execute($"new classlib -n {projectName}.LogicaServicio", root);
        Execute($"new nunit -n {projectName}.LogicaTest", root);
        Execute($"new sln -n SolucionGeneral", root);
        Execute($"sln add \"{projectName}.Api/{projectName}.Api.csproj\"", root);
        Execute($"sln add \"{projectName}.LogicaServicio/{projectName}.LogicaServicio.csproj\"", root);
        Execute($"sln add \"{projectName}.LogicaTest/{projectName}.LogicaTest.csproj\"", root);
        Execute(
            $"add \"{projectName}.Api/{projectName}.Api.csproj\" reference \"{projectName}.LogicaServicio/{projectName}.LogicaServicio.csproj\"",
            root
        );
        Execute(
            $"add \"{projectName}.LogicaTest/{projectName}.LogicaTest.csproj\" reference \"{projectName}.LogicaServicio/{projectName}.LogicaServicio.csproj\"",
            root
        );

        CreateWebApiSampleFiles(root, projectName);

        AnsiConsole.MarkupLine(
            $"[green]Solución Web API '{projectName}' creada correctamente[/]"
        );
    }

    private static void CreateWebApiSampleFiles(
        string root,
        string projectName)
    {
        var apiPath = Path.Combine(root, $"{projectName}.Api");
        var logicPath = Path.Combine(root, $"{projectName}.LogicaServicio");
        var appProgram = Path.Combine(apiPath, "Program.cs");
        var controllerPath = Path.Combine(apiPath, "Controllers");
        var serviceFolder = Path.Combine(logicPath, "Servicios");
        var entityFolder = Path.Combine(logicPath, "Entidades");

        Directory.CreateDirectory(controllerPath);
        Directory.CreateDirectory(serviceFolder);
        Directory.CreateDirectory(entityFolder);

        var serviceFile = Path.Combine(serviceFolder, "ProductoService.cs");
        File.WriteAllText(serviceFile,
$"using System.Collections.Generic;\nusing System.Linq;\nusing {projectName}.LogicaServicio.Entidades;\n\nnamespace {projectName}.LogicaServicio.Servicios;\n\npublic class ProductoService\n{{\n    private readonly List<Producto> _productos = new()\n    {{\n        new Producto {{ Id = 1, Nombre = \"Laptop\", Precio = 1500m, Categoria = \"Electrónica\" }},\n        new Producto {{ Id = 2, Nombre = \"Mouse\", Precio = 25m, Categoria = \"Periféricos\" }},\n    }};\n\n    public List<Producto> ObtenerTodos() => _productos.ToList();\n\n    public Producto? ObtenerPorId(int id) => _productos.FirstOrDefault(p => p.Id == id);\n\n    public void Agregar(Producto producto)\n    {{\n        producto.Id = _productos.Any() ? _productos.Max(p => p.Id) + 1 : 1;\n        _productos.Add(producto);\n    }}\n\n    public bool Actualizar(int id, Producto productoActualizado)\n    {{\n        var existente = ObtenerPorId(id);\n        if (existente == null) return false;\n\n        existente.Nombre = productoActualizado.Nombre;\n        existente.Precio = productoActualizado.Precio;\n        existente.Categoria = productoActualizado.Categoria;\n        return true;\n    }}\n\n    public bool Eliminar(int id)\n    {{\n        var existente = ObtenerPorId(id);\n        if (existente == null) return false;\n\n        _productos.Remove(existente);\n        return true;\n    }}\n}}\n");

        var entityFile = Path.Combine(entityFolder, "Producto.cs");
        File.WriteAllText(entityFile,
$"using System.ComponentModel.DataAnnotations;\n\nnamespace {projectName}.LogicaServicio.Entidades;\n\npublic class Producto\n{{\n    public int Id {{ get; set; }}\n\n    [Required(ErrorMessage = \"El nombre es obligatorio\")]\n    [StringLength(100, MinimumLength = 2)]\n    public string Nombre {{ get; set; }} = null!;\n\n    [Range(0.01, 1000000, ErrorMessage = \"El precio debe ser mayor a 0\")]\n    public decimal Precio {{ get; set; }}\n\n    [Required(ErrorMessage = \"La categoría es obligatoria\")]\n    public string Categoria {{ get; set; }} = null!;\n}}\n");

        var controllerFile = Path.Combine(controllerPath, "ProductoController.cs");
        File.WriteAllText(controllerFile,
$"using Microsoft.AspNetCore.Mvc;\nusing {projectName}.LogicaServicio.Entidades;\nusing {projectName}.LogicaServicio.Servicios;\n\nnamespace {projectName}.Api.Controllers;\n\n[ApiController]\n[Route(\"api/[controller]\")]\npublic class ProductoController : ControllerBase\n{{\n    private readonly ProductoService _servicio;\n\n    public ProductoController(ProductoService servicio)\n    {{\n        _servicio = servicio;\n    }}\n\n    [HttpGet]\n    public IActionResult ObtenerTodos() => Ok(_servicio.ObtenerTodos());\n\n    [HttpGet(\"{{id}}\")]\n    public IActionResult ObtenerPorId(int id)\n    {{\n        var producto = _servicio.ObtenerPorId(id);\n        if (producto == null)\n            return NotFound();\n\n        return Ok(producto);\n    }}\n\n    [HttpPost]\n    public IActionResult Crear([FromBody] Producto producto)\n    {{\n        _servicio.Agregar(producto);\n        return CreatedAtAction(nameof(ObtenerPorId), new {{ id = producto.Id }}, producto);\n    }}\n\n    [HttpPut(\"{{id}}\")]\n    public IActionResult Actualizar(int id, [FromBody] Producto producto)\n    {{\n        if (!_servicio.Actualizar(id, producto))\n            return NotFound();\n\n        return Ok(producto);\n    }}\n\n    [HttpDelete(\"{{id}}\")]\n    public IActionResult Eliminar(int id)\n    {{\n        if (!_servicio.Eliminar(id))\n            return NotFound();\n\n        return NoContent();\n    }}\n}}\n");

        if (File.Exists(Path.Combine(apiPath, "WeatherForecast.cs")))
        {
            File.Delete(Path.Combine(apiPath, "WeatherForecast.cs"));
        }

        if (File.Exists(Path.Combine(controllerPath, "WeatherForecastController.cs")))
        {
            File.Delete(Path.Combine(controllerPath, "WeatherForecastController.cs"));
        }

        if (File.Exists(appProgram))
        {
            var programText = File.ReadAllText(appProgram);
            programText = programText.Replace(
                "builder.Services.AddControllers();",
                $"builder.Services.AddControllers();{Environment.NewLine}builder.Services.AddSingleton<{projectName}.LogicaServicio.Servicios.ProductoService>();"
            );
            File.WriteAllText(appProgram, programText);
        }
    }

    private static void Execute(
        string arguments,
        string workingDirectory)
    {
        var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        string output =
            process.StandardOutput.ReadToEnd();

        string error =
            process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            AnsiConsole.MarkupLine(
                $"[red]{Markup.Escape(error)}[/]"
            );

            throw new Exception(
                $"Error ejecutando: dotnet {arguments}"
            );
        }
    }
}