namespace Mvp.Core.Infrastructure.Processes;

public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
