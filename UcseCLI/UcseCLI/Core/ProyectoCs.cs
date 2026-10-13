using System.Text.RegularExpressions;

namespace UcseCLI.Core;

/// <summary>
/// Lectura y edición puntual de archivos .csproj, y deducción del namespace
/// que corresponde a una carpeta dentro de un proyecto.
/// </summary>
public static class ProyectoCs
{
    private static readonly Regex ProjectReferencePattern =
        new(@"<ProjectReference\s+Include=""([^""]+)""", RegexOptions.Compiled);

    private static readonly Regex RootNamespacePattern =
        new(@"<RootNamespace>\s*([^<\s]+)\s*</RootNamespace>", RegexOptions.Compiled);

    /// <summary>
    /// Busca el .csproj más cercano partiendo de <paramref name="directorio"/> y
    /// subiendo por las carpetas padre. Devuelve null si no encuentra ninguno.
    /// </summary>
    public static string? BuscarCsprojMasCercano(string directorio, int nivelesMaximos = 8)
    {
        var actual = new DirectoryInfo(directorio);

        for (var nivel = 0; nivel < nivelesMaximos && actual is not null; nivel++)
        {
            if (actual.Exists)
            {
                var csproj = actual
                    .EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly)
                    .OrderBy(archivo => archivo.Name, StringComparer.Ordinal)
                    .FirstOrDefault();

                if (csproj is not null)
                {
                    return csproj.FullName;
                }
            }

            actual = actual.Parent;
        }

        return null;
    }

    /// <summary>
    /// Deduce el namespace de una carpeta: namespace raíz del proyecto más las
    /// carpetas intermedias (por ejemplo "Ventas.LogicaServicio.Entidades").
    /// Devuelve null si la carpeta no pertenece a ningún proyecto.
    /// </summary>
    public static string? DeducirNamespace(string directorioDestino)
    {
        var csproj = BuscarCsprojMasCercano(directorioDestino);

        if (csproj is null)
        {
            return null;
        }

        var raiz = LeerRootNamespace(csproj) ?? Path.GetFileNameWithoutExtension(csproj);
        var carpetaProyecto = Path.GetDirectoryName(csproj)!;
        var relativa = Path.GetRelativePath(carpetaProyecto, directorioDestino);

        var segmentos = new List<string> { raiz };

        if (relativa != "." && !relativa.StartsWith("..", StringComparison.Ordinal))
        {
            foreach (var parte in relativa.Split(
                         new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                var segmento = Naming.ASegmentoDeNamespace(parte);

                if (segmento is not null)
                {
                    segmentos.Add(segmento);
                }
            }
        }

        return string.Join('.', segmentos);
    }

    /// <summary>
    /// Devuelve el proyecto al que pertenece una carpeta, con su namespace raíz
    /// y el namespace que corresponde a la carpeta. Null si no hay .csproj.
    /// </summary>
    public static ContextoDeProyecto? ObtenerContexto(string directorioDestino)
    {
        var csproj = BuscarCsprojMasCercano(directorioDestino);

        if (csproj is null)
        {
            return null;
        }

        var raiz = LeerRootNamespace(csproj) ?? Path.GetFileNameWithoutExtension(csproj);
        var deCarpeta = DeducirNamespace(directorioDestino) ?? raiz;

        return new ContextoDeProyecto(csproj, raiz, deCarpeta);
    }

    /// <summary>
    /// Nombres de los proyectos referenciados por un .csproj
    /// (por ejemplo, el proyecto de lógica que usa la Web API).
    /// </summary>
    public static IReadOnlyList<string> LeerProyectosReferenciados(string rutaCsproj)
    {
        try
        {
            if (!File.Exists(rutaCsproj))
            {
                return Array.Empty<string>();
            }

            var contenido = File.ReadAllText(rutaCsproj);

            return ProjectReferencePattern
                .Matches(contenido)
                .Select(coincidencia => coincidencia.Groups[1].Value)
                .Select(ruta => Path.GetFileNameWithoutExtension(
                    ruta.Replace('\\', Path.DirectorySeparatorChar)))
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>Valor de RootNamespace declarado en el .csproj, si existe.</summary>
    public static string? LeerRootNamespace(string rutaCsproj)
    {
        try
        {
            var coincidencia = RootNamespacePattern.Match(File.ReadAllText(rutaCsproj));
            return coincidencia.Success ? coincidencia.Groups[1].Value : null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>
    /// Agrega un PackageReference al .csproj si todavía no está.
    /// Se edita el archivo en lugar de usar "dotnet add package" para no depender
    /// de la conexión a internet durante el armado del proyecto.
    /// </summary>
    public static bool AgregarPaquete(string rutaCsproj, string paquete, string version)
    {
        if (!File.Exists(rutaCsproj))
        {
            return false;
        }

        var contenido = File.ReadAllText(rutaCsproj);

        if (contenido.Contains($"Include=\"{paquete}\"", StringComparison.Ordinal))
        {
            return false;
        }

        var cierre = contenido.LastIndexOf("</Project>", StringComparison.Ordinal);

        if (cierre < 0)
        {
            return false;
        }

        var itemGroup =
            "  <ItemGroup>" + Environment.NewLine +
            $"    <PackageReference Include=\"{paquete}\" Version=\"{version}\" />" + Environment.NewLine +
            "  </ItemGroup>" + Environment.NewLine +
            Environment.NewLine;

        File.WriteAllText(rutaCsproj, contenido.Insert(cierre, itemGroup));

        return true;
    }

    /// <summary>Indica si el .csproj referencia un paquete determinado.</summary>
    public static bool TienePaquete(string rutaCsproj, string paquete)
    {
        try
        {
            return File.Exists(rutaCsproj)
                && File.ReadAllText(rutaCsproj)
                    .Contains($"Include=\"{paquete}\"", StringComparison.Ordinal);
        }
        catch (IOException)
        {
            return false;
        }
    }
}

/// <summary>Datos del proyecto que contiene una carpeta.</summary>
public sealed record ContextoDeProyecto(string RutaCsproj, string NamespaceRaiz, string NamespaceCarpeta)
{
    public string Nombre => Path.GetFileNameWithoutExtension(RutaCsproj);

    public string Carpeta => Path.GetDirectoryName(RutaCsproj)!;
}
