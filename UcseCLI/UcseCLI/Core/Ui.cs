using Spectre.Console;

namespace UcseCLI.Core;

/// <summary>
/// Salida por consola unificada. Todo el texto variable pasa por
/// <see cref="Markup.Escape"/> para que un nombre con corchetes no rompa el formato.
/// </summary>
public static class Ui
{
    public static void Titulo(string texto)
    {
        AnsiConsole.MarkupLine($"[bold cyan]{Markup.Escape(texto)}[/]");
        AnsiConsole.WriteLine();
    }

    public static void Subtitulo(string texto) =>
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(texto)}[/]");

    public static void Paso(string texto) =>
        AnsiConsole.MarkupLine($"[grey]»[/] {Markup.Escape(texto)}");

    public static void Ok(string texto) =>
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(texto)}");

    public static void Aviso(string texto) =>
        AnsiConsole.MarkupLine($"[yellow]![/] {Markup.Escape(texto)}");

    public static void Error(string texto, string? detalle = null)
    {
        AnsiConsole.MarkupLine($"[red]✗ {Markup.Escape(texto)}[/]");

        if (!string.IsNullOrWhiteSpace(detalle))
        {
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(detalle.Trim())}[/]");
        }
    }

    public static void Nota(string texto) =>
        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(texto)}[/]");

    public static void Linea() => AnsiConsole.WriteLine();

    /// <summary>Imprime un bloque de código sin interpretar markup.</summary>
    public static void Codigo(string codigo) => AnsiConsole.WriteLine(codigo);

    /// <summary>Lista de items con viñeta.</summary>
    public static void Items(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            AnsiConsole.MarkupLine($"  [grey]·[/] {Markup.Escape(item)}");
        }
    }

    /// <summary>Dos columnas alineadas (comando / descripción), sin depender de tablas.</summary>
    public static void Columnas(IEnumerable<(string Izquierda, string Derecha)> filas)
    {
        var lista = filas.ToList();

        if (lista.Count == 0)
        {
            return;
        }

        var ancho = lista.Max(fila => fila.Izquierda.Length);

        foreach (var (izquierda, derecha) in lista)
        {
            var etiqueta = izquierda.PadRight(ancho);
            AnsiConsole.MarkupLine($"  [cyan]{Markup.Escape(etiqueta)}[/]  {Markup.Escape(derecha)}");
        }
    }

    /// <summary>
    /// Muestra la estructura generada como árbol, a partir de rutas relativas
    /// con separador '/' (por ejemplo "Mi.Logica/Entidades/Producto.cs").
    /// </summary>
    public static void Estructura(string raiz, IEnumerable<string> rutasRelativas)
    {
        var arbol = new Tree($"[bold]{Markup.Escape(raiz)}[/]");
        var nodos = new Dictionary<string, TreeNode>(StringComparer.Ordinal);

        foreach (var ruta in rutasRelativas.OrderBy(r => r, StringComparer.Ordinal))
        {
            var partes = ruta.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var caminoAcumulado = string.Empty;
            TreeNode? padre = null;

            for (var i = 0; i < partes.Length; i++)
            {
                caminoAcumulado = caminoAcumulado.Length == 0
                    ? partes[i]
                    : caminoAcumulado + "/" + partes[i];

                if (!nodos.TryGetValue(caminoAcumulado, out var nodo))
                {
                    var esArchivo = i == partes.Length - 1;
                    var etiqueta = esArchivo
                        ? Markup.Escape(partes[i])
                        : $"[blue]{Markup.Escape(partes[i])}[/]";

                    nodo = padre is null ? arbol.AddNode(etiqueta) : padre.AddNode(etiqueta);
                    nodos[caminoAcumulado] = nodo;
                }

                padre = nodo;
            }
        }

        AnsiConsole.Write(arbol);
    }

    /// <summary>Bloque final con los comandos que conviene correr después.</summary>
    public static void ProximosPasos(IEnumerable<string> comandos)
    {
        var lista = comandos.ToList();

        if (lista.Count == 0)
        {
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Próximos pasos[/]");

        foreach (var comando in lista)
        {
            AnsiConsole.MarkupLine($"  [grey]$[/] [cyan]{Markup.Escape(comando)}[/]");
        }
    }

    /// <summary>
    /// Recordatorio de que lo generado es código de ejemplo: la herramienta
    /// automatiza el armado del proyecto, no resuelve el enunciado.
    /// </summary>
    public static void AvisoEjemplos(bool activos)
    {
        AnsiConsole.WriteLine();

        if (activos)
        {
            AnsiConsole.MarkupLine(
                "[yellow]El código de ejemplo quedó activo[/] [grey](compila y los tests corren).[/]");
            AnsiConsole.MarkupLine(
                "[grey]Al borrar la entidad de ejemplo vas a tener que borrar también lo que la usa.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[yellow]El código de ejemplo se generó comentado[/] [grey](por eso 'dotnet test' no corre ningún test todavía).[/]");
            AnsiConsole.MarkupLine(
                "[grey]Así podés borrar lo que no uses sin que el resto quede con errores.[/]");
            AnsiConsole.MarkupLine(
                "[grey]Para verlo funcionando: descomentá el bloque (Ctrl+K Ctrl+U) o generá con --ejemplos-activos.[/]");
        }

        AnsiConsole.MarkupLine(
            "[grey]El modelo y las reglas de negocio de tu enunciado los diseñás vos.[/]");
    }

    /// <summary>Aviso corto para los comandos que generan un archivo comentado.</summary>
    public static void AvisoArchivoComentado() =>
        AnsiConsole.MarkupLine(
            "[grey]Se generó comentado: descomentá el bloque (Ctrl+K Ctrl+U) o volvé a generarlo con --ejemplos-activos.[/]");

}
