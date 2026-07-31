using System.Diagnostics;
using Mvp.Core.Infrastructure.Processes;

namespace Mvp.Cli.Tests.Unit;

// Acceptance Test
// Traces to: L2-015, L2-062, L2-064
// Description: External tools use tokenized arguments and terminate promptly on cancellation.
public sealed class ProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_CancellationTerminatesLongRunningProcess()
    {
        var request = OperatingSystem.IsWindows()
            ? new ProcessRequest("powershell", ["-NoProfile", "-Command", "Start-Sleep -Seconds 30"])
            : new ProcessRequest("/bin/sh", ["-c", "sleep 30"]);
        using var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new ProcessRunner().RunAsync(request, source.Token));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
    }
}
