using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using Microsoft.Extensions.Logging;

namespace Mvp.Cli.Services;

public class InfrastructureGeneratorService : IInfrastructureGeneratorService
{
    private readonly IProjectFactory _projectFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ILogger<InfrastructureGeneratorService> _logger;

    public InfrastructureGeneratorService(IProjectFactory projectFactory, IArtifactGenerator artifactGenerator, ILogger<InfrastructureGeneratorService> logger)
    {
        _projectFactory = projectFactory;
        _artifactGenerator = artifactGenerator;
        _logger = logger;
    }

    public async Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating .NET Infrastructure project: {Name}", name);

        var project = await _projectFactory.CreateInfrastructure($"{name}.Infrastructure", outputPath);
        await _artifactGenerator.GenerateAsync(project);

        _logger.LogInformation("Successfully generated .NET Infrastructure project: {Name}.Infrastructure", name);
    }
}
