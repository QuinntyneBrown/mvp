using System.CommandLine;

namespace Mvp.Cli.Commands;

public static class NewCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("new", "Scaffold new MVP solution components");

        command.AddCommand(NewSolutionCommand.Create(services));
        command.AddCommand(NewApiCommand.Create(services));
        command.AddCommand(NewAppCommand.Create(services));

        return command;
    }
}
