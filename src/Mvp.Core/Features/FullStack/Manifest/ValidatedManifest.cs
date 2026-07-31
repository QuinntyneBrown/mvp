namespace Mvp.Core.Features.FullStack.Manifest;

public sealed record ValidatedManifest(
    string Name,
    string OutputRoot,
    IReadOnlyList<ValidatedEntity> Entities,
    IReadOnlyList<ValidatedPage> Pages,
    IReadOnlyList<ValidatedComponent> Components,
    IReadOnlyList<string> Warnings);
