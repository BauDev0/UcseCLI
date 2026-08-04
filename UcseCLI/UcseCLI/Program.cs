using System.CommandLine;

var rootCommand = new RootCommand(
    "UCSE CLI - Generador de proyectos"
);

rootCommand.Subcommands.Add(
    Commands.NewProjectCommand()
);

rootCommand.Subcommands.Add(
    Commands.NewFilesCommand()
);

rootCommand.Subcommands.Add(
    Commands.NewWebApiCommand()
);

return rootCommand.Parse(args).Invoke();