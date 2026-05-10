using CodeGenerator.DotNet.Artifacts.Solutions.Factories;
using CodeGenerator.DotNet.Artifacts.Solutions.Services;
using Microsoft.Extensions.Logging;

namespace Mvp.Cli.Services;

public class SolutionGeneratorService : ISolutionGeneratorService
{
    private readonly ISolutionFactory _solutionFactory;
    private readonly ISolutionService _solutionService;
    private readonly IAppGeneratorService _appGeneratorService;
    private readonly ILogger<SolutionGeneratorService> _logger;

    public SolutionGeneratorService(
        ISolutionFactory solutionFactory,
        ISolutionService solutionService,
        IAppGeneratorService appGeneratorService,
        ILogger<SolutionGeneratorService> logger)
    {
        _solutionFactory = solutionFactory;
        _solutionService = solutionService;
        _appGeneratorService = appGeneratorService;
        _logger = logger;
    }

    public async Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating full-stack MVP solution: {Name}", name);

        var model = await _solutionFactory.Create(name, $"{name}.Api", "webapi", string.Empty, outputPath);
        await _solutionService.Create(model);
        await _appGeneratorService.GenerateAsync(name, model.SrcDirectory, cancellationToken);

        _logger.LogInformation("Successfully generated full-stack MVP solution: {Name}", name);
        _logger.LogInformation("Solution created at: {Path}", model.SolutionDirectory);
    }
}
