namespace Mvp.Core.Errors;

public class GenerationException : Exception
{
    public GenerationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
