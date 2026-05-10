using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class InfrastructureGeneratorServiceTests
{
    private readonly Mock<IProjectFactory> _projectFactoryMock;
    private readonly Mock<IArtifactGenerator> _artifactGeneratorMock;
    private readonly Mock<ILogger<InfrastructureGeneratorService>> _loggerMock;
    private readonly InfrastructureGeneratorService _sut;

    public InfrastructureGeneratorServiceTests()
    {
        _projectFactoryMock = new Mock<IProjectFactory>();
        _artifactGeneratorMock = new Mock<IArtifactGenerator>();
        _loggerMock = new Mock<ILogger<InfrastructureGeneratorService>>();
        _sut = new InfrastructureGeneratorService(_projectFactoryMock.Object, _artifactGeneratorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_CallsProjectFactoryWithCorrectArgs()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);
            var project = new ProjectModel();
            _projectFactoryMock
                .Setup(f => f.CreateInfrastructure("Test.Infrastructure", tempDir))
                .ReturnsAsync(project);

            await _sut.GenerateAsync("Test", tempDir);

            _projectFactoryMock.Verify(
                f => f.CreateInfrastructure("Test.Infrastructure", tempDir),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_CallsArtifactGeneratorWithProject()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);
            var project = new ProjectModel();
            _projectFactoryMock
                .Setup(f => f.CreateInfrastructure(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(project);

            await _sut.GenerateAsync("MyInfrastructure", tempDir);

            _artifactGeneratorMock.Verify(
                g => g.GenerateAsync(project),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
