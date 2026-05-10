using CodeGenerator.DotNet.Artifacts.Solutions;
using CodeGenerator.DotNet.Artifacts.Solutions.Factories;
using CodeGenerator.DotNet.Artifacts.Solutions.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class SolutionGeneratorServiceTests
{
    private readonly Mock<ISolutionFactory> _solutionFactoryMock;
    private readonly Mock<ISolutionService> _solutionServiceMock;
    private readonly Mock<IAppGeneratorService> _appGeneratorMock;
    private readonly Mock<ILogger<SolutionGeneratorService>> _loggerMock;
    private readonly SolutionGeneratorService _sut;

    public SolutionGeneratorServiceTests()
    {
        _solutionFactoryMock = new Mock<ISolutionFactory>();
        _solutionServiceMock = new Mock<ISolutionService>();
        _appGeneratorMock = new Mock<IAppGeneratorService>();
        _loggerMock = new Mock<ILogger<SolutionGeneratorService>>();

        _sut = new SolutionGeneratorService(
            _solutionFactoryMock.Object,
            _solutionServiceMock.Object,
            _appGeneratorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_CallsSolutionFactoryAndService()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var model = new SolutionModel("TestSolution", tempDir);
            _solutionFactoryMock
                .Setup(f => f.Create("TestSolution", "TestSolution.Api", "webapi", string.Empty, tempDir))
                .ReturnsAsync(model);
            _solutionServiceMock
                .Setup(s => s.Create(model))
                .Returns(Task.CompletedTask);

            await _sut.GenerateAsync("TestSolution", tempDir);

            _solutionFactoryMock.Verify(
                f => f.Create("TestSolution", "TestSolution.Api", "webapi", string.Empty, tempDir),
                Times.Once);
            _solutionServiceMock.Verify(s => s.Create(model), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_CallsAppGenerator()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var model = new SolutionModel("MyApp", tempDir);
            _solutionFactoryMock
                .Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(model);

            await _sut.GenerateAsync("MyApp", tempDir);

            _appGeneratorMock.Verify(
                s => s.GenerateAsync("MyApp", model.SrcDirectory, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_WhenSolutionServiceThrows_PropagatesException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var model = new SolutionModel("FailSolution", tempDir);
            _solutionFactoryMock
                .Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(model);
            _solutionServiceMock
                .Setup(s => s.Create(It.IsAny<SolutionModel>()))
                .ThrowsAsync(new InvalidOperationException("solution creation failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GenerateAsync("FailSolution", tempDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_UsesSolutionSrcDirectoryForAppGenerator()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var model = new SolutionModel("ReadmeTest", tempDir);
            _solutionFactoryMock
                .Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(model);

            await _sut.GenerateAsync("ReadmeTest", tempDir);

            _appGeneratorMock.Verify(
                s => s.GenerateAsync("ReadmeTest", model.SrcDirectory, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
