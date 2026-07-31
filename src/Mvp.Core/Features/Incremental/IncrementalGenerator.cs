using System.Globalization;
using System.Reflection;
using DotLiquid;
using Mvp.Core.Errors;
using Mvp.Core.Features.FullStack.Manifest;
using Mvp.Core.Features.Generation;
using Mvp.Core.Infrastructure.Output;
using Mvp.Core.Infrastructure.Processes;

namespace Mvp.Core.Features.Incremental;

public sealed class IncrementalGenerator(
    ITransactionalGenerationOutput output,
    IProcessRunner processRunner) : IIncrementalGenerator
{
    private static readonly string ResourcePrefix = TemplateResources.Incremental;

    public Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken)
    {
        var name = ManifestValidator.ValidateIncrementalName(request.Name);
        string outputRoot;
        try
        {
            outputRoot = Path.GetFullPath(request.OutputRoot);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ManifestValidationException([$"Output path is invalid: {exception.Message}"]);
        }
        var target = Path.Combine(outputRoot, GetTargetName(request.Kind, name));
        return output.CommitAsync(
            target,
            request.Force,
            (stage, token) => GenerateInStageAsync(request with { Name = name, OutputRoot = outputRoot }, stage, token),
            cancellationToken);
    }

    private async Task GenerateInStageAsync(
        GenerationRequest request,
        string stage,
        CancellationToken cancellationToken)
    {
        if (request.Kind == GenerationKind.App && request.UseAngularCli)
        {
            await GenerateWithAngularCliAsync(request.Name, stage, cancellationToken).ConfigureAwait(false);
            return;
        }

        var tokens = new Dictionary<string, object>
        {
            ["Name"] = request.Name,
            ["NameKebab"] = Naming.ToKebabCase(request.Name),
            ["Library"] = GetLibraryName(request.Kind),
            ["LibraryLower"] = GetLibraryName(request.Kind).ToLowerInvariant(),
        };

        foreach (var entry in GetEntries(request.Kind))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Render(entry.RelativePath, tokens);
            var content = Render(ReadResource(entry.ResourceName), tokens);
            await SafeFileWriter.WriteAsync(stage, path, content, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task GenerateWithAngularCliAsync(
        string name,
        string stage,
        CancellationToken cancellationToken)
    {
        var executable = OperatingSystem.IsWindows() ? "ng.cmd" : "ng";
        ProcessResult result;
        try
        {
            result = await processRunner.RunAsync(
                new ProcessRequest(
                    executable,
                    [
                        "new",
                        Naming.ToKebabCase(name) + "-app",
                        "--directory",
                        ".",
                        "--routing",
                        "--style",
                        "scss",
                        "--standalone",
                        "--skip-git",
                        "--skip-install",
                        "--defaults",
                    ],
                    stage),
                cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException exception)
        {
            throw new ExternalToolException(
                "Angular CLI could not be started. Install Angular CLI or omit --use-angular-cli to use packaged templates.",
                exception);
        }

        if (result.ExitCode != 0)
        {
            var detail = string.IsNullOrWhiteSpace(result.StandardError)
                ? $"exit code {result.ExitCode}"
                : result.StandardError.Trim();
            throw new ExternalToolException($"Angular CLI generation failed: {detail}");
        }
    }

    private static string GetTargetName(GenerationKind kind, string name) => kind switch
    {
        GenerationKind.Solution => name,
        GenerationKind.Api => $"{name}.Api",
        GenerationKind.Core => $"{name}.Core",
        GenerationKind.Infrastructure => $"{name}.Infrastructure",
        GenerationKind.App => $"{name}.App",
        GenerationKind.ApiLibrary => $"{name}.Api",
        GenerationKind.ComponentsLibrary => $"{name}.Components",
        GenerationKind.DomainLibrary => $"{name}.Domain",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    private static string GetLibraryName(GenerationKind kind) => kind switch
    {
        GenerationKind.ApiLibrary => "Api",
        GenerationKind.ComponentsLibrary => "Components",
        GenerationKind.DomainLibrary => "Domain",
        _ => string.Empty,
    };

    private static TemplateEntry[] GetEntries(GenerationKind kind) => kind switch
    {
        GenerationKind.Solution =>
        [
            new("{{Name}}.sln", "Solution.sln.liquid"),
            new("src/{{Name}}.Api/{{Name}}.Api.csproj", "Api.csproj.liquid"),
            new("src/{{Name}}.Api/Program.cs", "ApiProgram.cs.liquid"),
            new("src/{{Name}}.Core/{{Name}}.Core.csproj", "Core.csproj.liquid"),
            new("src/{{Name}}.Core/Class1.cs", "CoreClass.cs.liquid"),
            new("src/{{Name}}.Infrastructure/{{Name}}.Infrastructure.csproj", "Infrastructure.csproj.liquid"),
            new("src/{{Name}}.Infrastructure/Class1.cs", "InfrastructureClass.cs.liquid"),
            .. AppEntries("src/{{Name}}.App/"),
        ],
        GenerationKind.Api =>
        [
            new("{{Name}}.Api.csproj", "Api.csproj.liquid"),
            new("Program.cs", "ApiProgram.cs.liquid"),
        ],
        GenerationKind.Core =>
        [
            new("{{Name}}.Core.csproj", "Core.csproj.liquid"),
            new("Class1.cs", "CoreClass.cs.liquid"),
        ],
        GenerationKind.Infrastructure =>
        [
            new("{{Name}}.Infrastructure.csproj", "InfrastructureStandalone.csproj.liquid"),
            new("Class1.cs", "InfrastructureClass.cs.liquid"),
        ],
        GenerationKind.App => AppEntries(string.Empty),
        GenerationKind.ApiLibrary or GenerationKind.ComponentsLibrary or GenerationKind.DomainLibrary =>
        [
            new("package.json", "LibraryPackage.json.liquid"),
            new("README.md", "LibraryReadme.md.liquid"),
            new("src/lib/public-api.ts", "LibraryPublicApi.ts.liquid"),
            new("src/lib/{{LibraryLower}}.service.ts", "LibraryService.ts.liquid"),
        ],
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    private static TemplateEntry[] AppEntries(string prefix) =>
    [
        new(prefix + "package.json", "AppPackage.json.liquid"),
        new(prefix + "angular.json", "AppAngular.json.liquid"),
        new(prefix + "tsconfig.json", "AppTsConfig.json.liquid"),
        new(prefix + "src/main.ts", "AppMain.ts.liquid"),
        new(prefix + "src/index.html", "AppIndex.html.liquid"),
        new(prefix + "src/styles.scss", "AppStyles.scss.liquid"),
        new(prefix + "src/app/app.component.ts", "AppComponent.ts.liquid"),
        new(prefix + "README.md", "AppReadme.md.liquid"),
    ];

    private static string ReadResource(string resourceName)
    {
        using var stream = typeof(IncrementalGenerator).Assembly.GetManifestResourceStream(ResourcePrefix + resourceName)
            ?? throw new GenerationException($"Packaged template resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string Render(string template, IDictionary<string, object> tokens) =>
        Template.Parse(template).Render(Hash.FromDictionary(tokens), CultureInfo.InvariantCulture);

    private sealed record TemplateEntry(string RelativePath, string ResourceName);
}
