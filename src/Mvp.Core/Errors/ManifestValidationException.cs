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
