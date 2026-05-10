namespace Mvp.Cli.Services;

public interface ISolutionGeneratorService
{
    Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default);
}
