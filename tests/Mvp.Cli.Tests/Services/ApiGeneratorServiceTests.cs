using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class ApiGeneratorServiceTests
{
    private readonly Mock<IProcessRunner> _processRunnerMock;
    private readonly Mock<ILogger<ApiGeneratorService>> _loggerMock;
    private readonly ApiGeneratorService _sut;

    public ApiGeneratorServiceTests()
    {
        _processRunnerMock = new Mock<IProcessRunner>();
        _loggerMock = new Mock<ILogger<ApiGeneratorService>>();
        _sut = new ApiGeneratorService(_processRunnerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_CreatesApiDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);
            _processRunnerMock
                .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            await _sut.GenerateAsync("TestApi", tempDir);

            Assert.True(Directory.Exists(Path.Combine(tempDir, "TestApi.Api")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_CallsDotnetNewWebapi()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);
            _processRunnerMock
                .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            await _sut.GenerateAsync("MyApi", tempDir);

            _processRunnerMock.Verify(
                r => r.RunAsync("dotnet", It.Is<string>(a => a.Contains("webapi") && a.Contains("MyApi.Api")), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_WhenProcessFails_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);
            _processRunnerMock
                .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

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
