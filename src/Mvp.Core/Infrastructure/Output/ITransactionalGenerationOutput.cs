using Mvp.Core.Features.Generation;

namespace Mvp.Core.Infrastructure.Output;

public interface ITransactionalGenerationOutput
{
    Task<GenerationResult> CommitAsync(
        string targetPath,
        bool force,
        Func<string, CancellationToken, Task> generateInStagingDirectory,
        CancellationToken cancellationToken);
}
