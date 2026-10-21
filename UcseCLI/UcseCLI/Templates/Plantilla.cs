namespace UcseCLI.Templates;

/// <summary>
/// Reemplazo de marcadores en las plantillas. Las plantillas se escriben como
/// texto literal con marcadores del estilo __ENTIDAD__, así el código de ejemplo
/// se lee igual que el archivo que se va a generar.
/// </summary>
public static class Plantilla
{
    public static string Render(string plantilla, params (string Marcador, string Valor)[] valores)
    {
        var texto = plantilla;

        foreach (var (marcador, valor) in valores)
        {
            texto = texto.Replace(marcador, valor, StringComparison.Ordinal);
        }

        return texto;
    }

    /// <summary>Encabezado opcional con el namespace con ámbito de archivo (Unidad 2).</summary>
    public static string Namespace(string? espacioDeNombres) =>
        string.IsNullOrWhiteSpace(espacioDeNombres)
            ? string.Empty
            : $"namespace {espacioDeNombres};{Environment.NewLine}{Environment.NewLine}";

    /// <summary>
    /// Bloque de directivas using. Cada línea termina con un salto, así la
    /// plantilla puede dejar la línea siguiente en blanco sin importar si el
    /// bloque quedó vacío.
    /// </summary>
    public static string Usings(params string?[] espaciosDeNombres)
    {
        var lineas = espaciosDeNombres
            .Where(ns => !string.IsNullOrWhiteSpace(ns))
            .Select(ns => ns!.Trim())
            .Distinct(StringComparer.Ordinal)
            .Select(ns => $"using {ns};{Environment.NewLine}");

        return string.Concat(lineas);
    }

    /// <summary>
    /// Convierte un archivo de ejemplo en comentarios, con un encabezado que
    /// explica cómo activarlo. Se comenta línea por línea (y no con un bloque
    /// /* */) para que el editor pueda alternarlo con un atajo y para poder
    /// descomentar solo una parte.
    /// </summary>
    public static string Comentar(string codigo, string titulo, string? aclaracion = null)
    {
        var regla = "// " + new string('=', 72);

        var lineasDelEncabezado = new List<string>
        {
            regla,
            $"// EJEMPLO DE REFERENCIA · {titulo}",
            "//",
            "// Está comentado a propósito: podés borrar este archivo —o cualquier parte",
            "// de él— sin que el resto del proyecto quede con errores.",
            "//",
            "// Para activarlo: seleccioná el bloque y descomentá",
            "//   VS Code / Visual Studio → Ctrl+K Ctrl+U  (Ctrl+/ alterna)",
            "// Para que se genere ya activo: ucse <comando> ... --ejemplos-activos"
        };

        if (!string.IsNullOrWhiteSpace(aclaracion))
        {
            lineasDelEncabezado.Add("//");
            lineasDelEncabezado.Add("// " + aclaracion.Trim());
        }

        lineasDelEncabezado.Add(regla);
        lineasDelEncabezado.Add(string.Empty);
        lineasDelEncabezado.Add(string.Empty);

        var encabezado = string.Join(Environment.NewLine, lineasDelEncabezado);

        var lineas = codigo
            .Replace("\r\n", "\n")
            .TrimEnd('\n')
            .Split('\n')
            .Select(linea => linea.Length == 0 ? "//" : "// " + linea);

        return encabezado + string.Join(Environment.NewLine, lineas);
    }

    /// <summary>Comenta un bloque de texto, sin encabezado.</summary>
    public static string ComentarBloque(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return string.Empty;
        }

        var lineas = texto
            .Replace("\r\n", "\n")
            .TrimEnd('\n')
            .Split('\n')
            .Select(linea => linea.Length == 0 ? "//" : "// " + linea);

        return string.Join(Environment.NewLine, lineas) + Environment.NewLine;
    }

    /// <summary>
    /// Devuelve el código tal cual o comentado, según lo pedido por el usuario.
    /// </summary>
    public static string SegunModo(string codigo, string titulo, bool activo) =>
        activo ? codigo : Comentar(codigo, titulo);
}
