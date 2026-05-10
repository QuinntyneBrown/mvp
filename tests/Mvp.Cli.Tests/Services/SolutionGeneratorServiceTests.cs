using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class SolutionGeneratorServiceTests
{
    private readonly Mock<IProcessRunner> _processRunnerMock;
    private readonly Mock<IApiGeneratorService> _apiGeneratorMock;
    private readonly Mock<IAppGeneratorService> _appGeneratorMock;
    private readonly Mock<ILogger<SolutionGeneratorService>> _loggerMock;
    private readonly SolutionGeneratorService _sut;

    public SolutionGeneratorServiceTests()
    {
        _processRunnerMock = new Mock<IProcessRunner>();
        _apiGeneratorMock = new Mock<IApiGeneratorService>();
        _appGeneratorMock = new Mock<IAppGeneratorService>();
        _loggerMock = new Mock<ILogger<SolutionGeneratorService>>();

        _sut = new SolutionGeneratorService(
            _processRunnerMock.Object,
            _apiGeneratorMock.Object,
            _appGeneratorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_CreatesDirectoryAndCallsProcessRunner()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            _processRunnerMock
                .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            await _sut.GenerateAsync("TestSolution", tempDir);

            Assert.True(Directory.Exists(Path.Combine(tempDir, "TestSolution")));
            Assert.True(Directory.Exists(Path.Combine(tempDir, "TestSolution", "src")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_CallsApiAndAppGenerators()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            _processRunnerMock
                .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            await _sut.GenerateAsync("MyApp", tempDir);

            _apiGeneratorMock.Verify(
                s => s.GenerateAsync("MyApp", It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _appGeneratorMock.Verify(
                s => s.GenerateAsync("MyApp", It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_WhenProcessRunnerFails_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            _processRunnerMock
                .Setup(r => r.RunAsync("dotnet", It.IsRegex("new sln"), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

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
    public async Task GenerateAsync_CreatesReadme()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            _processRunnerMock
                .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            await _sut.GenerateAsync("ReadmeTest", tempDir);

            var readmePath = Path.Combine(tempDir, "ReadmeTest", "README.md");
            Assert.True(File.Exists(readmePath));
            var content = await File.ReadAllTextAsync(readmePath);
            Assert.Contains("ReadmeTest", content);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
