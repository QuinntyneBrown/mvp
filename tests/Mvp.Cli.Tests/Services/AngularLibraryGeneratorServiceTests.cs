using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using Xunit;

namespace Mvp.Cli.Tests.Services;

public class AngularLibraryGeneratorServiceTests
{
    private readonly Mock<ILogger<AngularLibraryGeneratorService>> _loggerMock;
    private readonly AngularLibraryGeneratorService _sut;

    public AngularLibraryGeneratorServiceTests()
    {
        _loggerMock = new Mock<ILogger<AngularLibraryGeneratorService>>();
        _sut = new AngularLibraryGeneratorService(_loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_ForApiLibrary_CreatesServiceAndContractFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);

            await _sut.GenerateAsync("TestApp", tempDir, "Api");

            var libraryPath = Path.Combine(tempDir, "TestApp.Api", "src", "lib");
            Assert.True(File.Exists(Path.Combine(libraryPath, "api.service.ts")));
            Assert.True(File.Exists(Path.Combine(libraryPath, "api.service.contract.ts")));
            Assert.True(File.Exists(Path.Combine(libraryPath, "public-api.ts")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_ForComponentsLibrary_CreatesSplitComponentFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);

            await _sut.GenerateAsync("TestApp", tempDir, "Components");

            var componentPath = Path.Combine(tempDir, "TestApp.Components", "src", "lib", "test-app-card");
            Assert.True(File.Exists(Path.Combine(componentPath, "test-app-card.component.ts")));
            Assert.True(File.Exists(Path.Combine(componentPath, "test-app-card.component.html")));
            Assert.True(File.Exists(Path.Combine(componentPath, "test-app-card.component.scss")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GenerateAsync_ForDomainLibrary_CreatesServiceAndContractFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);

            await _sut.GenerateAsync("TestApp", tempDir, "Domain");

            var libraryPath = Path.Combine(tempDir, "TestApp.Domain", "src", "lib");
            Assert.True(File.Exists(Path.Combine(libraryPath, "domain.service.ts")));
            Assert.True(File.Exists(Path.Combine(libraryPath, "domain.service.contract.ts")));
            Assert.True(File.Exists(Path.Combine(libraryPath, "public-api.ts")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
