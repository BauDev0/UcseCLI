using System.CommandLine;
using UcseCLI;
public static class Commands
{
    public static Command NewProjectCommand()
    {
        var command = new Command(
            "new",
            "Crear solución completa"
        );

        var nameArgument =
            new Argument<string>("name");

        command.Arguments.Add(nameArgument);

        command.SetAction(result =>
        {
            var name =
                result.GetValue(nameArgument);

            ProjectGenerator.CreateSolution(
                name!
            );

            return 0;
        });

        return command;
    }

    public static Command AddFilesCommand()
    {
        var command = new Command(
            "add-files",
            "Crear múltiples archivos"
        );

        var pathArgument =
            new Argument<string>("path");

        var filesArgument =
            new Argument<string[]>("files");

        command.Arguments.Add(pathArgument);
        command.Arguments.Add(filesArgument);

        command.SetAction(result =>
        {
            var path =
                result.GetValue(pathArgument);

            var files =
                result.GetValue(filesArgument);

            FileGenerator.CreateFiles(
                path!,
                files!
            );

            return 0;
        });

        return command;
    }
}