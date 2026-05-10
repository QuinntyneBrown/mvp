using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class AppGeneratorServiceTests
{
    private readonly Mock<IProcessRunner> _processRunnerMock;
    private readonly Mock<ILogger<AppGeneratorService>> _loggerMock;
    private readonly AppGeneratorService _sut;

    public AppGeneratorServiceTests()
    {
        _processRunnerMock = new Mock<IProcessRunner>();
        _loggerMock = new Mock<ILogger<AppGeneratorService>>();
        _sut = new AppGeneratorService(_processRunnerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_WhenNgCliNotAvailable_CreatesMinimalAngularStructure()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);

            // Simulate ng not available
            _processRunnerMock
                .Setup(r => r.RunAsync("ng", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("ng not found"));

            await _sut.GenerateAsync("TestApp", tempDir);

            var appDir = Path.Combine(tempDir, "TestApp.App");
            Assert.True(Directory.Exists(appDir));
            Assert.True(File.Exists(Path.Combine(appDir, "package.json")));
            Assert.True(File.Exists(Path.Combine(appDir, "tsconfig.json")));
            Assert.True(File.Exists(Path.Combine(appDir, "angular.json")));
            Assert.True(File.Exists(Path.Combine(appDir, "src", "main.ts")));
            Assert.True(File.Exists(Path.Combine(appDir, "src", "index.html")));
            Assert.True(File.Exists(Path.Combine(appDir, "src", "app", "app.module.ts")));
            Assert.True(File.Exists(Path.Combine(appDir, "src", "app", "app.component.ts")));
            Assert.True(File.Exists(Path.Combine(appDir, "src", "app", "app-routing.module.ts")));
            Assert.True(File.Exists(Path.Combine(appDir, "src", "environments", "environment.ts")));
            Assert.True(File.Exists(Path.Combine(appDir, "src", "environments", "environment.prod.ts")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_PackageJsonContainsAppName()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);

            _processRunnerMock
                .Setup(r => r.RunAsync("ng", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("ng not found"));

            await _sut.GenerateAsync("MyProject", tempDir);

            var packageJson = await File.ReadAllTextAsync(Path.Combine(tempDir, "MyProject.App", "package.json"));
            Assert.Contains("myproject-app", packageJson);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_IndexHtmlContainsAppName()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);

            _processRunnerMock
                .Setup(r => r.RunAsync("ng", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("ng not found"));

            await _sut.GenerateAsync("MyProject", tempDir);

            var indexHtml = await File.ReadAllTextAsync(Path.Combine(tempDir, "MyProject.App", "src", "index.html"));
            Assert.Contains("MyProject", indexHtml);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
