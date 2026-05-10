using System.CommandLine;

namespace Mvp.Cli.Commands;

public static class NewCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("new", "Scaffold new MVP solution components");

        command.AddCommand(NewSolutionCommand.Create(services));
        command.AddCommand(NewApiCommand.Create(services));
        command.AddCommand(NewCoreCommand.Create(services));
        command.AddCommand(NewInfrastructureCommand.Create(services));
        command.AddCommand(NewAppCommand.Create(services));
        command.AddCommand(NewApiLibraryCommand.Create(services));
        command.AddCommand(NewComponentsLibraryCommand.Create(services));
        command.AddCommand(NewDomainLibraryCommand.Create(services));
        command.AddCommand(NewDotNetAngularJwtAuthenticatedMvpCommand.Create(services));

        return command;
    }
}
