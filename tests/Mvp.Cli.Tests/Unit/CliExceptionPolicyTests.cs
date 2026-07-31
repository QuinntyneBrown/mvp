using Mvp.Cli.Bootstrap;

namespace Mvp.Cli.Tests.Unit;

// Acceptance Test
// Traces to: L2-003, L2-061, L2-062
// Description: The centralized policy returns stable codes and hides internal detail by default.
public sealed class CliExceptionPolicyTests
{
    [Theory]
    [MemberData(nameof(Cases))]
    public async Task ExecuteAsync_MapsTypedFailures(Exception exception, int expectedExitCode)
    {
        using var error = new StringWriter();

        var exitCode = await new CliExceptionPolicy().ExecuteAsync(
            _ => Task.FromException(exception),
            error,
            diagnostic: false,
            CancellationToken.None);

        Assert.Equal(expectedExitCode, exitCode);
        Assert.NotEmpty(error.ToString());
        Assert.DoesNotContain(" at ", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_DiagnosticIncludesInternalExceptionDetail()
    {
        using var error = new StringWriter();

        var exitCode = await new CliExceptionPolicy().ExecuteAsync(
            _ => Task.FromException(new InvalidOperationException("diagnostic marker")),
            error,
            diagnostic: true,
            CancellationToken.None);

        Assert.Equal(CliExitCodes.InternalError, exitCode);
        Assert.Contains("InvalidOperationException", error.ToString(), StringComparison.Ordinal);
        Assert.Contains("diagnostic marker", error.ToString(), StringComparison.Ordinal);
    }

    public static TheoryData<Exception, int> Cases => new()
    {
        { new ManifestValidationException(["invalid"]), CliExitCodes.InvalidInput },
        { new GenerationConflictException("conflict"), CliExitCodes.Conflict },
        { new GenerationException("generation"), CliExitCodes.GenerationFailure },
        { new InvalidOperationException("internal detail"), CliExitCodes.InternalError },
        { new OperationCanceledException(), CliExitCodes.Cancelled },
    };
}
