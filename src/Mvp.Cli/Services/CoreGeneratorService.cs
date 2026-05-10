using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using Microsoft.Extensions.Logging;

namespace Mvp.Cli.Services;

public class CoreGeneratorService : ICoreGeneratorService
{
    private readonly IProjectFactory _projectFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ILogger<CoreGeneratorService> _logger;

    public CoreGeneratorService(IProjectFactory projectFactory, IArtifactGenerator artifactGenerator, ILogger<CoreGeneratorService> logger)
    {
        _projectFactory = projectFactory;
        _artifactGenerator = artifactGenerator;
        _logger = logger;
    }

    public async Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating .NET Core project: {Name}", name);

        var project = await _projectFactory.CreateCore($"{name}.Core", outputPath);
        await _artifactGenerator.GenerateAsync(project);

        _logger.LogInformation("Successfully generated .NET Core project: {Name}.Core", name);
    }
}
