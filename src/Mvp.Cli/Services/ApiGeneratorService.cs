using Microsoft.Extensions.Logging;

namespace Mvp.Cli.Services;

public class ApiGeneratorService : IApiGeneratorService
{
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<ApiGeneratorService> _logger;

    public ApiGeneratorService(IProcessRunner processRunner, ILogger<ApiGeneratorService> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task GenerateAsync(string name, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating .NET Web API project: {Name}", name);

        var apiDir = Path.Combine(outputPath, $"{name}.Api");
        Directory.CreateDirectory(apiDir);

        var exitCode = await _processRunner.RunAsync(
            "dotnet",
            $"new webapi -n {name}.Api --no-openapi -o \"{apiDir}\"",
            outputPath,
            cancellationToken);

        if (exitCode != 0)
            throw new InvalidOperationException($"Failed to create .NET Web API project '{name}.Api'. Exit code: {exitCode}");

        _logger.LogInformation("Successfully generated .NET Web API project: {Name}.Api", name);
    }
}
