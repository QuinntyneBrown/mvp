namespace Mvp.Core.Features.FullStack.Manifest;

public sealed record ValidatedEntity(string Name, IReadOnlyList<ValidatedProperty> Properties);
