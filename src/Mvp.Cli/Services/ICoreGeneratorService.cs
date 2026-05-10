namespace Mvp.Cli.Services;

public interface ICoreGeneratorService
{
    Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default);
}
