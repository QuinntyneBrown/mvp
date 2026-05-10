namespace Mvp.Cli.Services;

public interface IAppGeneratorService
{
    Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default);
}
