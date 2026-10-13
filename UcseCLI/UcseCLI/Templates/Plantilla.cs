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
}
