namespace Mvp.Cli.Bootstrap;

public static class CliExitCodes
{
    public const int Success = 0;
    public const int InvalidInput = 1;
    public const int Conflict = 2;
    public const int GenerationFailure = 3;
    public const int InternalError = 70;
    public const int Cancelled = 130;
}
