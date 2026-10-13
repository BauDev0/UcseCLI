using System.Text.RegularExpressions;

namespace UcseCLI.Core;

/// <summary>
/// Validación y normalización de nombres siguiendo los estándares de la materia
/// (Unidad 2 - sección 18: clases y propiedades en PascalCase, parámetros y
/// variables locales en camelCase, interfaces con prefijo I).
/// </summary>
public static class Naming
{
    // Identificador C# válido: empieza con letra o guión bajo, sin separadores de ruta.
    private static readonly Regex IdentifierPattern =
        new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    // Nombre de proyecto/carpeta: igual que un identificador, pero debe empezar con letra.
    private static readonly Regex ProjectNamePattern =
        new(@"^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.Compiled);

    // Target framework moniker: net8.0, net9.0, net10.0, ...
    private static readonly Regex FrameworkPattern =
        new(@"^net[0-9]+\.[0-9]+$", RegexOptions.Compiled);

    private static readonly HashSet<string> PalabrasReservadas = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
        "checked", "class", "const", "continue", "decimal", "default", "delegate",
        "do", "double", "else", "enum", "event", "explicit", "extern", "false",
        "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
        "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
        "new", "null", "object", "operator", "out", "override", "params", "private",
        "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    /// <summary>Indica si el texto es un identificador C# válido y no una palabra reservada.</summary>
    public static bool EsIdentificadorValido(string? nombre) =>
        !string.IsNullOrWhiteSpace(nombre)
        && IdentifierPattern.IsMatch(nombre)
        && !PalabrasReservadas.Contains(nombre);

    /// <summary>Indica si el texto sirve como nombre de solución/proyecto.</summary>
    public static bool EsNombreDeProyectoValido(string? nombre) =>
        !string.IsNullOrWhiteSpace(nombre)
        && ProjectNamePattern.IsMatch(nombre)
        && !PalabrasReservadas.Contains(nombre);

    /// <summary>Indica si el texto es un target framework válido (por ejemplo net8.0).</summary>
    public static bool EsFrameworkValido(string? framework) =>
        !string.IsNullOrWhiteSpace(framework) && FrameworkPattern.IsMatch(framework);

    /// <summary>Convierte un nombre a PascalCase: "mi producto" / "mi_producto" => "MiProducto".</summary>
    public static string APascalCase(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return nombre;
        }

        var partes = nombre.Split(new[] { ' ', '_', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
        var resultado = string.Concat(partes.Select(parte =>
            char.ToUpperInvariant(parte[0]) + (parte.Length > 1 ? parte[1..] : string.Empty)));

        return resultado.Length == 0 ? nombre : resultado;
    }

    /// <summary>Convierte un nombre a camelCase: "Producto" => "producto".</summary>
    public static string ACamelCase(string nombre)
    {
        var pascal = APascalCase(nombre);

        if (string.IsNullOrEmpty(pascal))
        {
            return pascal;
        }

        return char.ToLowerInvariant(pascal[0]) + (pascal.Length > 1 ? pascal[1..] : string.Empty);
    }

    /// <summary>Nombre de campo privado según la convención usada en los apuntes: "_producto".</summary>
    public static string ACampoPrivado(string nombre) => "_" + ACamelCase(nombre);

    /// <summary>
    /// Plural simple en castellano, usado para nombres de listas y archivos JSON
    /// ("Producto" => "productos"). Es una aproximación: si queda raro, renombralo.
    /// </summary>
    public static string APlural(string nombre)
    {
        var singular = ACamelCase(nombre);

        if (string.IsNullOrEmpty(singular))
        {
            return singular;
        }

        var ultima = char.ToLowerInvariant(singular[^1]);

        if (ultima == 's')
        {
            return singular;
        }

        if (ultima == 'z')
        {
            return singular[..^1] + "ces";
        }

        return singular + "s";
    }

    /// <summary>
    /// Convierte el nombre de una carpeta en un segmento de namespace válido.
    /// Devuelve null si la carpeta no puede representarse (por ejemplo "bin.obj").
    /// </summary>
    public static string? ASegmentoDeNamespace(string carpeta)
    {
        if (string.IsNullOrWhiteSpace(carpeta))
        {
            return null;
        }

        var limpio = new string(carpeta
            .Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : ' ')
            .ToArray());

        var segmento = APascalCase(limpio);

        if (segmento.Length == 0 || char.IsDigit(segmento[0]))
        {
            return null;
        }

        return segmento;
    }

    /// <summary>Asegura el prefijo "I" que la materia pide para las interfaces.</summary>
    public static string ANombreDeInterfaz(string nombre)
    {
        var pascal = APascalCase(nombre);

        if (pascal.Length >= 2 && pascal[0] == 'I' && char.IsUpper(pascal[1]))
        {
            return pascal;
        }

        return "I" + pascal;
    }
}
