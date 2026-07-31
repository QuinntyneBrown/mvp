using Mvp.Core.Features.FullStack.Manifest;
using Mvp.Core.Features.Generation;

namespace Mvp.Core.Features.FullStack.Generation;

public interface IFullStackGenerator
{
    Task<GenerationResult> GenerateAsync(
        ValidatedManifest manifest,
        bool force,
        CancellationToken cancellationToken);
}
