namespace Mvp.Core.Features.Generation;

public sealed record GenerationRequest(
    GenerationKind Kind,
    string Name,
    string OutputRoot,
    bool Force = false,
    bool UseAngularCli = false);
