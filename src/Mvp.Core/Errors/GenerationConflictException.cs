namespace Mvp.Core.Errors;

public class GenerationConflictException : Exception
{
    public GenerationConflictException(string message) : base(message)
    {
    }
}
