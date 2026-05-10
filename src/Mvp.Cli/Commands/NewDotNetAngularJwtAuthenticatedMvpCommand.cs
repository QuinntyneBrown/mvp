using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mvp.Cli.Manifests;
using Mvp.Cli.Services;
using System.CommandLine;

namespace Mvp.Cli.Commands;

public static class NewDotNetAngularJwtAuthenticatedMvpCommand
{
    public static Command Create(IServiceProvider services)
    {
        var nameOption = new Option<string?>(
            aliases: ["--name", "-n"],
            description: "Solution name (required unless provided in --config).");

        var outputOption = new Option<string?>(
            aliases: ["--output", "-o"],
            description: "Target directory. Defaults to the current working directory. The solution is created as a subfolder named after --name.");

        var configOption = new Option<string?>(
            aliases: ["--config", "-c"],
            description: "Path to a YAML manifest describing entities, pages, and components.");

        var command = new Command(
            "dotnet-angular-jwt-mvp",
            "Scaffold a JWT-authenticated full-stack .NET + Angular MVP, grounded in the patterns documented in docs/technology-guidance-and-practices.md.");

        command.AddOption(nameOption);
        command.AddOption(outputOption);
        command.AddOption(configOption);

        command.SetHandler(async (string? name, string? output, string? configPath) =>
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            var generator = services.GetRequiredService<IDotNetAngularJwtAuthenticatedMvpGeneratorService>();
            var manifestLoader = services.GetRequiredService<IMvpManifestLoader>();

            try
            {
                var manifest = configPath is null
                    ? new MvpManifest()
                    : manifestLoader.Load(configPath);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    manifest.Name = name;
                }

                if (string.IsNullOrWhiteSpace(manifest.Name))
                {
                    logger.LogError("A solution name is required. Provide --name or set 'name:' in the manifest.");
                    Environment.Exit(1);
                    return;
                }

                var outputDirectory = output ?? manifest.Output ?? Directory.GetCurrentDirectory();
                Directory.CreateDirectory(outputDirectory);

                await generator.GenerateAsync(manifest, outputDirectory);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to scaffold JWT-authenticated MVP: {Message}", ex.Message);
                Environment.Exit(1);
            }
        }, nameOption, outputOption, configOption);

        return command;
    }
}
