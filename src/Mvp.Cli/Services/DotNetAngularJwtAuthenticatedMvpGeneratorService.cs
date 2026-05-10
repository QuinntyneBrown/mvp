using CodeGenerator.DotNet.Artifacts.JwtAuthMvp;
using Microsoft.Extensions.Logging;
using Mvp.Cli.Manifests;

namespace Mvp.Cli.Services;

public class DotNetAngularJwtAuthenticatedMvpGeneratorService : IDotNetAngularJwtAuthenticatedMvpGeneratorService
{
    private readonly IJwtAuthenticatedMvpFactory _factory;
    private readonly ILogger<DotNetAngularJwtAuthenticatedMvpGeneratorService> _logger;

    public DotNetAngularJwtAuthenticatedMvpGeneratorService(
        IJwtAuthenticatedMvpFactory factory,
        ILogger<DotNetAngularJwtAuthenticatedMvpGeneratorService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task GenerateAsync(MvpManifest manifest, string outputDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var solutionRoot = Path.Combine(outputDirectory, manifest.Name);
        _logger.LogInformation("Generating JWT-authenticated MVP '{Name}' under '{Root}'.", manifest.Name, solutionRoot);

        var options = new JwtAuthenticatedMvpOptions
        {
            Name = manifest.Name,
            Directory = solutionRoot,
            Entities = manifest.Entities.Select(e => new JwtAuthMvpEntity
            {
                Name = e.Name,
                Properties = e.Properties.Select(p => new JwtAuthMvpProperty
                {
                    Name = p.Name,
                    Type = p.Type,
                }).ToList(),
            }).ToList(),
            Pages = manifest.Pages.Select(p => new JwtAuthMvpFrontendPage
            {
                Name = p.Name,
                Route = p.Route ?? string.Empty,
                RequiresAuth = p.RequiresAuth,
            }).ToList(),
            Components = manifest.Components.Select(c => new JwtAuthMvpFrontendComponent
            {
                Name = c.Name,
                Library = c.Library,
            }).ToList(),
        };

        await _factory.GenerateAsync(options, cancellationToken);

        _logger.LogInformation("Generated JWT-authenticated MVP at '{Root}'.", solutionRoot);
    }
}
