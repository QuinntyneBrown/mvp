using System.CommandLine;
using Mvp.Cli.Bootstrap;
using Mvp.Cli.Features.FullStack.Generation;
using Mvp.Cli.Features.FullStack.Manifest;
using Mvp.Cli.Features.Generation;

namespace Mvp.Cli.Commands;

public sealed class RootCommandFactory(
    IIncrementalGenerator incrementalGenerator,
    IFullStackGenerator fullStackGenerator,
    IManifestLoader manifestLoader,
    ManifestValidator manifestValidator,
    CliExceptionPolicy exceptionPolicy)
{
    public RootCommand Create()
    {
        var diagnosticOption = new Option<bool>("--diagnostic")
        {
            Description = "Include stack traces and internal details in errors.",
            Recursive = true,
        };
        var root = new RootCommand("Create maintainable .NET 10 and Angular 22 MVP solutions.");
        root.Options.Add(diagnosticOption);
        root.Subcommands.Add(CreateNewCommand(diagnosticOption));
        return root;
    }

    private Command CreateNewCommand(Option<bool> diagnosticOption)
    {
        var command = new Command("new", "Generate a solution or one of its constituent projects.");
        command.Subcommands.Add(CreateIncremental("solution", "Create a .NET solution, API, core, infrastructure, and Angular app.", GenerationKind.Solution, diagnosticOption));
        command.Subcommands.Add(CreateIncremental("api", "Create a .NET Web API project.", GenerationKind.Api, diagnosticOption));
        command.Subcommands.Add(CreateIncremental("core", "Create a .NET core class library.", GenerationKind.Core, diagnosticOption));
        command.Subcommands.Add(CreateIncremental("infrastructure", "Create a .NET infrastructure class library.", GenerationKind.Infrastructure, diagnosticOption));
        command.Subcommands.Add(CreateIncremental("app", "Create an Angular application.", GenerationKind.App, diagnosticOption, supportsAngularCli: true));
        command.Subcommands.Add(CreateIncremental("api-library", "Create an Angular API library.", GenerationKind.ApiLibrary, diagnosticOption));
        command.Subcommands.Add(CreateIncremental("components-library", "Create an Angular components library.", GenerationKind.ComponentsLibrary, diagnosticOption));
        command.Subcommands.Add(CreateIncremental("domain-library", "Create an Angular domain library.", GenerationKind.DomainLibrary, diagnosticOption));
        command.Subcommands.Add(CreateFullStackCommand(diagnosticOption));
        return command;
    }

    private Command CreateIncremental(
        string commandName,
        string description,
        GenerationKind kind,
        Option<bool> diagnosticOption,
        bool supportsAngularCli = false)
    {
        var nameOption = NameOption(required: true);
        var outputOption = OutputOption();
        var forceOption = ForceOption();
        var angularCliOption = new Option<bool>("--use-angular-cli")
        {
            Description = "Use the installed Angular CLI instead of packaged templates.",
        };
        var command = new Command(commandName, description);
        command.Options.Add(nameOption);
        command.Options.Add(outputOption);
        command.Options.Add(forceOption);
        if (supportsAngularCli)
        {
            command.Options.Add(angularCliOption);
        }

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            return await exceptionPolicy.ExecuteAsync(async token =>
            {
                await output.WriteLineAsync($"Generating {commandName}...").ConfigureAwait(false);
                var request = new GenerationRequest(
                    kind,
                    parseResult.GetValue(nameOption)!,
                    parseResult.GetValue(outputOption) ?? Directory.GetCurrentDirectory(),
                    parseResult.GetValue(forceOption),
                    supportsAngularCli && parseResult.GetValue(angularCliOption));
                var result = await incrementalGenerator.GenerateAsync(request, token).ConfigureAwait(false);
                await WriteResultAsync(result, output).ConfigureAwait(false);
            }, error, parseResult.GetValue(diagnosticOption), cancellationToken).ConfigureAwait(false);
        });
        return command;
    }

    private Command CreateFullStackCommand(Option<bool> diagnosticOption)
    {
        var nameOption = NameOption(required: false);
        var outputOption = OutputOption();
        var configOption = new Option<string?>("--config", "-c")
        {
            Description = "YAML manifest describing entities, pages, and components.",
        };
        var forceOption = ForceOption();
        var command = new Command(
            "dotnet-angular-jwt-mvp",
            "Create a JWT-authenticated .NET 10 and Angular 22 solution from options or YAML.");
        command.Options.Add(nameOption);
        command.Options.Add(outputOption);
        command.Options.Add(configOption);
        command.Options.Add(forceOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var output = parseResult.InvocationConfiguration.Output;
            var error = parseResult.InvocationConfiguration.Error;
            return await exceptionPolicy.ExecuteAsync(async token =>
            {
                await output.WriteLineAsync("Generating dotnet-angular-jwt-mvp...").ConfigureAwait(false);
                var configPath = parseResult.GetValue(configOption);
                var loaded = configPath is null
                    ? new ManifestLoadResult(new Manifests.MvpManifest(), [])
                    : manifestLoader.Load(configPath);
                var manifest = manifestValidator.Validate(
                    loaded.Manifest,
                    parseResult.GetValue(nameOption),
                    parseResult.GetValue(outputOption),
                    loaded.Warnings);
                foreach (var warning in manifest.Warnings)
                {
                    await error.WriteLineAsync($"warning: {warning}").ConfigureAwait(false);
                }

                var result = await fullStackGenerator.GenerateAsync(
                    manifest,
                    parseResult.GetValue(forceOption),
                    token).ConfigureAwait(false);
                await WriteResultAsync(result, output).ConfigureAwait(false);
            }, error, parseResult.GetValue(diagnosticOption), cancellationToken).ConfigureAwait(false);
        });
        return command;
    }

    private static Option<string?> NameOption(bool required) => new("--name", "-n")
    {
        Description = "PascalCase solution or project name.",
        Required = required,
    };

    private static Option<string?> OutputOption() => new("--output", "-o")
    {
        Description = "Output directory. Missing parent directories are created after validation.",
    };

    private static Option<bool> ForceOption() => new("--force", "-f")
    {
        Description = "Replace an existing target using a recoverable backup and atomic move.",
    };

    private static async Task WriteResultAsync(GenerationResult result, TextWriter output)
    {
        await output.WriteLineAsync($"Generated {result.Artifacts.Count} files at '{result.TargetPath}'.").ConfigureAwait(false);
        if (result.ReplacedExistingOutput)
        {
            await output.WriteLineAsync($"Replaced the existing target '{result.TargetPath}' using transactional backup and rollback.")
                .ConfigureAwait(false);
        }

        foreach (var warning in result.Warnings)
        {
            await output.WriteLineAsync($"warning: {warning}").ConfigureAwait(false);
        }
    }
}
