using Mvp.Cli.Features.FullStack.Manifest;
using Mvp.Cli.Features.Generation;

namespace Mvp.Cli.Features.FullStack.Generation;

public interface IFullStackGenerator
{
    Task<GenerationResult> GenerateAsync(
        ValidatedManifest manifest,
        bool force,
        CancellationToken cancellationToken);
}
