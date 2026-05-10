using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using Microsoft.Extensions.Logging;

namespace Mvp.Cli.Services;

public class ApiGeneratorService : IApiGeneratorService
{
    private readonly IProjectFactory _projectFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ILogger<ApiGeneratorService> _logger;

    public ApiGeneratorService(IProjectFactory projectFactory, IArtifactGenerator artifactGenerator, ILogger<ApiGeneratorService> logger)
    {
        _projectFactory = projectFactory;
        _artifactGenerator = artifactGenerator;
        _logger = logger;
    }

    public async Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating .NET Web API project: {Name}", name);

        var project = await _projectFactory.CreateWebApi($"{name}.Api", outputPath);
        await _artifactGenerator.GenerateAsync(project);

        _logger.LogInformation("Successfully generated .NET Web API project: {Name}.Api", name);
    }
}
