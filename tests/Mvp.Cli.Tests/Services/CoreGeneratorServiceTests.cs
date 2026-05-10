using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class CoreGeneratorServiceTests
{
    private readonly Mock<IProjectFactory> _projectFactoryMock;
    private readonly Mock<IArtifactGenerator> _artifactGeneratorMock;
    private readonly Mock<ILogger<CoreGeneratorService>> _loggerMock;
    private readonly CoreGeneratorService _sut;

    public CoreGeneratorServiceTests()
    {
        _projectFactoryMock = new Mock<IProjectFactory>();
        _artifactGeneratorMock = new Mock<IArtifactGenerator>();
        _loggerMock = new Mock<ILogger<CoreGeneratorService>>();
        _sut = new CoreGeneratorService(_projectFactoryMock.Object, _artifactGeneratorMock.Object, _loggerMock.Object);
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
                .Setup(f => f.CreateCore("Test.Core", tempDir))
                .ReturnsAsync(project);

            await _sut.GenerateAsync("Test", tempDir);

            _projectFactoryMock.Verify(
                f => f.CreateCore("Test.Core", tempDir),
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
                .Setup(f => f.CreateCore(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(project);

            await _sut.GenerateAsync("MyCore", tempDir);

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
