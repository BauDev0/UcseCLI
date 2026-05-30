using Spectre.Console;

public static class FileGenerator
{
    public static void CreateFiles(
        string path,
        string[] files)
    {
        Directory.CreateDirectory(path);

        foreach (var file in files)
        {
            var fullPath =
                Path.Combine(
                    path,
                    $"{file}.cs"
                );

            File.WriteAllText(
                fullPath,
            $@"public class {file}
            {{
            }}
");
        }

        AnsiConsole.MarkupLine(
            $"[green]{files.Length} archivos creados[/]"
        );
    }
}