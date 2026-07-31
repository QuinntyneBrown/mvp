namespace Mvp.Core.Features.Generation;

public interface IIncrementalGenerator
{
    Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken);
}
