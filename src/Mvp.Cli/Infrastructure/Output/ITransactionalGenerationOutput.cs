using Mvp.Cli.Features.Generation;

namespace Mvp.Cli.Infrastructure.Output;

public interface ITransactionalGenerationOutput
{
    Task<GenerationResult> CommitAsync(
        string targetPath,
        bool force,
        Func<string, CancellationToken, Task> generateInStagingDirectory,
        CancellationToken cancellationToken);
}
