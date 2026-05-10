namespace Mvp.Cli.Services;

public interface IProcessRunner
{
    Task<int> RunAsync(string command, string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default);
}
