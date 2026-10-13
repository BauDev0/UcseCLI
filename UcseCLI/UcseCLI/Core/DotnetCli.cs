using System.ComponentModel;
using System.Diagnostics;

namespace UcseCLI.Core;

/// <summary>Resultado de ejecutar un comando del SDK de .NET.</summary>
public sealed record ResultadoDotnet(int CodigoSalida, string Salida, string Error)
{
    public bool Exitoso => CodigoSalida == 0;
}

/// <summary>
/// Se lanza cuando un comando "dotnet" falla o cuando el SDK no está instalado.
/// La herramienta la captura para mostrar un mensaje entendible en lugar de un stack trace.
/// </summary>
public sealed class DotnetException : Exception
{
    public DotnetException(string mensaje, string? detalle = null) : base(mensaje)
    {
        Detalle = detalle;
    }

    public string? Detalle { get; }
}

/// <summary>
/// Ejecuta comandos del SDK de .NET. Los argumentos se pasan de a uno
/// (ArgumentList) para no depender del entrecomillado del shell.
/// </summary>
public static class DotnetCli
{
    /// <summary>Ejecuta "dotnet ..." y devuelve el resultado sin lanzar excepciones por código de salida.</summary>
    public static ResultadoDotnet Ejecutar(string directorioDeTrabajo, params string[] argumentos)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = directorioDeTrabajo,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argumento in argumentos)
        {
            startInfo.ArgumentList.Add(argumento);
        }

        try
        {
            using var proceso = Process.Start(startInfo)
                ?? throw new DotnetException("No se pudo iniciar el proceso 'dotnet'.");

            // Se leen las dos salidas en paralelo para que un buffer lleno no trabe el proceso.
            var salidaTask = proceso.StandardOutput.ReadToEndAsync();
            var errorTask = proceso.StandardError.ReadToEndAsync();

            proceso.WaitForExit();

            return new ResultadoDotnet(
                proceso.ExitCode,
                salidaTask.GetAwaiter().GetResult(),
                errorTask.GetAwaiter().GetResult());
        }
        catch (Win32Exception)
        {
            throw new DotnetException(
                "No se encontró el SDK de .NET ('dotnet' no está en el PATH).",
                "Instalalo desde https://dotnet.microsoft.com/download y volvé a intentar.");
        }
    }

    /// <summary>Ejecuta "dotnet ..." y lanza <see cref="DotnetException"/> si falla.</summary>
    public static void EjecutarOFallar(string directorioDeTrabajo, params string[] argumentos)
    {
        var resultado = Ejecutar(directorioDeTrabajo, argumentos);

        if (resultado.Exitoso)
        {
            return;
        }

        var detalle = string.IsNullOrWhiteSpace(resultado.Error)
            ? resultado.Salida
            : resultado.Error;

        throw new DotnetException(
            $"Falló el comando: dotnet {string.Join(' ', argumentos)}",
            detalle?.Trim());
    }

    /// <summary>Versión del SDK instalada, o null si no se pudo determinar.</summary>
    public static string? ObtenerVersion()
    {
        try
        {
            var resultado = Ejecutar(Directory.GetCurrentDirectory(), "--version");
            return resultado.Exitoso ? resultado.Salida.Trim() : null;
        }
        catch (DotnetException)
        {
            return null;
        }
    }

    /// <summary>SDKs instalados, uno por línea.</summary>
    public static IReadOnlyList<string> ListarSdks()
    {
        try
        {
            var resultado = Ejecutar(Directory.GetCurrentDirectory(), "--list-sdks");

            if (!resultado.Exitoso)
            {
                return Array.Empty<string>();
            }

            return resultado.Salida
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(linea => linea.Trim())
                .Where(linea => linea.Length > 0)
                .ToList();
        }
        catch (DotnetException)
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Agrega "-f net8.0" a los argumentos de "dotnet new" cuando el usuario
    /// indicó un framework. Si no lo indicó, se usa el default del SDK instalado.
    /// </summary>
    public static string[] ConFramework(string[] argumentos, string? framework)
    {
        if (string.IsNullOrWhiteSpace(framework))
        {
            return argumentos;
        }

        return argumentos.Concat(new[] { "-f", framework }).ToArray();
    }
}
