namespace Mvp.Cli.Services;

public interface IInfrastructureGeneratorService
{
    Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default);
}
