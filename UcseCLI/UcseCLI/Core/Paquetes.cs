namespace UcseCLI.Core;

/// <summary>
/// Paquetes NuGet que la herramienta agrega a los proyectos generados.
/// Las versiones están fijas para que lo generado compile igual en cualquier máquina.
/// </summary>
public static class Paquetes
{
    /// <summary>Serialización JSON usada en la Unidad 5.</summary>
    public const string NewtonsoftJson = "Newtonsoft.Json";

    public const string VersionNewtonsoftJson = "13.0.3";
}
