namespace Mvp.Cli.Services;

public interface IAngularLibraryGeneratorService
{
    Task GenerateAsync(string name, string outputPath, string libraryName, CancellationToken cancellationToken = default);
}
