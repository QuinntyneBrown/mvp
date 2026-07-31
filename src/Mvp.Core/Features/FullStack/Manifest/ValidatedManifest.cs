namespace Mvp.Core.Features.FullStack.Manifest;

public enum FrontendLibrary
{
    Api,
    Components,
    Domain,
}

public sealed record ValidatedProperty(string Name, string Type);

public sealed record ValidatedEntity(string Name, IReadOnlyList<ValidatedProperty> Properties);

public sealed record ValidatedPage(string Name, string Route, bool RequiresAuth);

public sealed record ValidatedComponent(string Name, FrontendLibrary Library);

public sealed record ValidatedManifest(
    string Name,
    string OutputRoot,
    IReadOnlyList<ValidatedEntity> Entities,
    IReadOnlyList<ValidatedPage> Pages,
    IReadOnlyList<ValidatedComponent> Components,
    IReadOnlyList<string> Warnings);
