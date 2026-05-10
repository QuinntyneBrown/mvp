using Mvp.Cli.Manifests;

namespace Mvp.Cli.Services;

public interface IDotNetAngularJwtAuthenticatedMvpGeneratorService
{
    Task GenerateAsync(MvpManifest manifest, string outputDirectory, CancellationToken cancellationToken = default);
}
