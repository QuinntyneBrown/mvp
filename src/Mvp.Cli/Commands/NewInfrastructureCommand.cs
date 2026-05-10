using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mvp.Cli.Services;
using System.CommandLine;

namespace Mvp.Cli.Commands;

public static class NewInfrastructureCommand
{
    public static Command Create(IServiceProvider services)
    {
        var nameOption = new Option<string>(
            aliases: ["--name", "-n"],
            description: "The name of the Infrastructure project")
        {
            IsRequired = true
        };

        var outputOption = new Option<string>(
            aliases: ["--output", "-o"],
            description: "The output directory (defaults to current directory)",
            getDefaultValue: Directory.GetCurrentDirectory);

        var command = new Command("infrastructure", "Creates a new .NET Infrastructure project");
        command.AddOption(nameOption);
        command.AddOption(outputOption);

        command.SetHandler(async (string name, string output) =>
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            var generator = services.GetRequiredService<IInfrastructureGeneratorService>();

            try
            {
                await generator.GenerateAsync(name, output);
            }
            catch (Exception ex)
            {
                logger.LogError("Error creating Infrastructure project '{Name}': {Message}", name, ex.Message);
                Environment.Exit(1);
            }
        }, nameOption, outputOption);

        return command;
    }
}
