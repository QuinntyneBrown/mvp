namespace Mvp.Cli.Services;

public interface IApiGeneratorService
{
    Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default);
}
