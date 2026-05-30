using Spectre.Console;
using System.Diagnostics;

namespace UcseCLI;

public static class ProjectGenerator
{
    public static void CreateSolution(
        string projectName)
    {
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

        Execute(
            $"dotnet new classlib -n {projectName}.LogicaServicio",
            root
        );

        Execute(
            $"dotnet new nunit -n {projectName}.LogicaTest",
            root
        );

        Execute(
            $"dotnet new sln -n SolucionGeneral",
            root
        );

        Execute(
            $"dotnet sln add \"{projectName}.LogicaServicio/{projectName}.LogicaServicio.csproj\"",
            root
        );

        Execute(
            $"dotnet sln add \"{projectName}.LogicaTest/{projectName}.LogicaTest.csproj\"",
            root
        );

        Execute(
            $"dotnet add \"{projectName}.LogicaTest/{projectName}.LogicaTest.csproj\" reference \"{projectName}.LogicaServicio/{projectName}.LogicaServicio.csproj\"",
            root
        );

        AnsiConsole.MarkupLine(
            $"[green]Proyecto '{projectName}' creado correctamente[/]"
        );
    }

    private static void Execute(
        string command,
        string workingDirectory)
    {
        var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
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
                $"[red]{error}[/]"
            );

            throw new Exception(
                $"Error ejecutando: {command}"
            );
        }
    }
}