using System.ComponentModel;
using System.Diagnostics;

namespace UcseCLI.Core;

/// <summary>
/// Operaciones mínimas de Git (Unidad 3): inicializar el repositorio de la
/// solución recién creada. Si Git no está instalado, no se interrumpe el armado.
/// </summary>
public static class Git
{
    /// <summary>
    /// Ejecuta "git init" en la carpeta indicada, dejando "main" como rama por
    /// defecto. Devuelve false si Git no está disponible o el comando falló.
    /// </summary>
    public static bool Inicializar(string directorio)
    {
        // -b main requiere Git 2.28 o posterior; si no está soportado se reintenta sin el flag.
        if (Ejecutar(directorio, "init", "-b", "main"))
        {
            return true;
        }

        return Ejecutar(directorio, "init");
    }

    /// <summary>Indica si la carpeta (o alguna de sus padres) ya es un repositorio.</summary>
    public static bool EsRepositorio(string directorio)
    {
        var actual = new DirectoryInfo(directorio);

        while (actual is not null)
        {
            if (Directory.Exists(Path.Combine(actual.FullName, ".git")))
            {
                return true;
            }

            actual = actual.Parent;
        }

        return false;
    }

    private static bool Ejecutar(string directorio, params string[] argumentos)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = directorio,
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
            using var proceso = Process.Start(startInfo);

            if (proceso is null)
            {
                return false;
            }

            // Se vacían las salidas para que el proceso no quede bloqueado.
            var salidaTask = proceso.StandardOutput.ReadToEndAsync();
            var errorTask = proceso.StandardError.ReadToEndAsync();

            proceso.WaitForExit();

            salidaTask.GetAwaiter().GetResult();
            errorTask.GetAwaiter().GetResult();

            return proceso.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            // Git no está instalado o no está en el PATH.
            return false;
        }
    }
}
