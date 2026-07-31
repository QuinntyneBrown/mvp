namespace Mvp.Cli.Features.Generation;

public enum GenerationKind
{
    Solution,
    Api,
    Core,
    Infrastructure,
    App,
    ApiLibrary,
    ComponentsLibrary,
    DomainLibrary,
}

public sealed record GenerationRequest(
    GenerationKind Kind,
    string Name,
    string OutputRoot,
    bool Force = false,
    bool UseAngularCli = false);

public sealed record GenerationResult(
    string TargetPath,
    IReadOnlyList<string> Artifacts,
    bool ReplacedExistingOutput,
    IReadOnlyList<string> Warnings);

public interface IIncrementalGenerator
{
    Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken);
}
