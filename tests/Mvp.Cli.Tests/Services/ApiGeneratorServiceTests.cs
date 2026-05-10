using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class ApiGeneratorServiceTests
{
    private readonly Mock<IProjectFactory> _projectFactoryMock;
    private readonly Mock<IArtifactGenerator> _artifactGeneratorMock;
    private readonly Mock<ILogger<ApiGeneratorService>> _loggerMock;
    private readonly ApiGeneratorService _sut;

    public ApiGeneratorServiceTests()
    {
        _projectFactoryMock = new Mock<IProjectFactory>();
        _artifactGeneratorMock = new Mock<IArtifactGenerator>();
        _loggerMock = new Mock<ILogger<ApiGeneratorService>>();
        _sut = new ApiGeneratorService(_projectFactoryMock.Object, _artifactGeneratorMock.Object, _loggerMock.Object);
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
                .Setup(f => f.CreateWebApi("TestApi.Api", tempDir, null))
                .ReturnsAsync(project);

            await _sut.GenerateAsync("TestApi", tempDir);

            _projectFactoryMock.Verify(
                f => f.CreateWebApi("TestApi.Api", tempDir, null),
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
                .Setup(f => f.CreateWebApi(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(project);

            await _sut.GenerateAsync("MyApi", tempDir);

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

    [Fact]
    public async Task GenerateAsync_WhenArtifactGeneratorThrows_PropagatesException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);
            _projectFactoryMock
                .Setup(f => f.CreateWebApi(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(new ProjectModel());
            _artifactGeneratorMock
                .Setup(g => g.GenerateAsync(It.IsAny<object>()))
                .ThrowsAsync(new InvalidOperationException("generation failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GenerateAsync("FailApi", tempDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
