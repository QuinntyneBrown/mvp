namespace Mvp.Core.Features.Generation;

public sealed record GenerationResult(
    string TargetPath,
    IReadOnlyList<string> Artifacts,
    bool ReplacedExistingOutput,
    IReadOnlyList<string> Warnings);
