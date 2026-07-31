namespace Mvp.Core.Errors;

public class ManifestValidationException : Exception
{
    public ManifestValidationException(IEnumerable<string> errors)
        : base("Input validation failed.")
    {
        Errors = errors.ToArray();
    }

    public IReadOnlyList<string> Errors { get; }
}

public class GenerationConflictException : Exception
{
    public GenerationConflictException(string message) : base(message)
    {
    }
}

public class GenerationException : Exception
{
    public GenerationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class ExternalToolException : GenerationException
{
    public ExternalToolException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
