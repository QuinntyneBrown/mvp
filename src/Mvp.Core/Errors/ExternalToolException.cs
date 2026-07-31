namespace Mvp.Core.Errors;

public sealed class ExternalToolException : GenerationException
{
    public ExternalToolException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
